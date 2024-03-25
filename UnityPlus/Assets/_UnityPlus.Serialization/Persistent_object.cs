using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Text;
using System.Threading.Tasks;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace UnityPlus.Serialization
{
	public static class Persistent_object
	{
		private struct PersistentField
		{
			public FieldInfo f;
			public PersistAttribute attr;

			public PersistentField( FieldInfo f, PersistAttribute attr )
			{
				this.f = f;
				this.attr = attr;
			}
		}

		private struct PersistentProperty
		{
			public PropertyInfo p;
			public PersistAttribute attr;

			public PersistentProperty( PropertyInfo p, PersistAttribute attr )
			{
				this.p = p;
				this.attr = attr;
			}
		}

		private static readonly Dictionary<Type, (PersistentField[] fields, PersistentProperty[] properties)> _referencepersistentMembers = new();
		private static readonly Dictionary<Type, (PersistentField[] fields, PersistentProperty[] properties)> _datapersistentMembers = new();

		private static ((PersistentField[] fields, PersistentProperty[] properties) refT, (PersistentField[] fields, PersistentProperty[] properties) dataT) CacheType( Type type )
		{
			List<PersistentField> finalDataFields = new();
			List<PersistentField> finalReferenceFields = new();
			List<PersistentProperty> finalDataProperties = new();
			List<PersistentProperty> finalReferenceProperties = new();

			FieldInfo[] fields = type.GetFields( BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic );
			foreach( var field in fields )
			{
				PersistAttribute attr = field.GetCustomAttribute<PersistAttribute>();
				if( attr != null )
				{
					if( attr.PersistsReference )
						finalReferenceFields.Add( new PersistentField( field, attr ) );
					if( attr.PersistsData )
						finalDataFields.Add( new PersistentField(field, attr) );
				}
			}

			PropertyInfo[] properties = type.GetProperties( BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic );
			foreach( var property in properties )
			{
				PersistAttribute attr = property.GetCustomAttribute<PersistAttribute>();
				if( attr != null )
				{
					if( attr.PersistsReference )
						finalReferenceProperties.Add( new PersistentProperty(property, attr) );
					if( attr.PersistsData )
						finalDataProperties.Add( new PersistentProperty(property, attr) );
				}
			}

			var refTuple = (finalReferenceFields.ToArray(), finalReferenceProperties.ToArray());
			var dataTuple = (finalDataFields.ToArray(), finalDataProperties.ToArray());

			_referencepersistentMembers.Add( type, refTuple );
			_datapersistentMembers.Add( type, dataTuple );
			return (refTuple, dataTuple);
		}

		//
		//	REFERENCES
		//

		private struct Entry
		{
			public readonly Func<SerializedObject, IForwardReferenceMap, object> ToInstanceFuncs;
			public readonly Func<object, IReverseReferenceMap, SerializedObject> ToSerializedFuncs;

			//public readonly Func<object, IReverseReferenceMap, SerializedData> GetDataFuncs;
			//public readonly Action<object, IForwardReferenceMap, SerializedData> SetDataFuncs;
		}

		private static readonly TypeMap<Entry> _cache = new();

		private static readonly Dictionary<Type, MethodInfo> _getDatas = new(); // TODO - replace with Func<...> and make a lambda to bridge.
		private static readonly Dictionary<Type, MethodInfo> _setDatas = new();

		// there are basically 3 different ways to serialize data - IPersistsData, auto-serialization, and the extension methods.

		// can we ensure that the data for the endpoint ends up on the correct endpoint?
		// technically, the data for the endpoint should be stored separately.

		public static void CacheMethods()
		{
			_getDatas.Clear();
			_setDatas.Clear();

			List<Type> availableContainingClasses = AppDomain.CurrentDomain.GetAssemblies()
				.SelectMany( a => a.GetTypes() )
				.Where( dt => !dt.IsSealed && !dt.IsGenericType )
				.ToList();

			foreach( var cls in availableContainingClasses )
			{
				MethodInfo[] methods = cls.GetMethods( BindingFlags.Public | BindingFlags.Static );

				foreach( var method in methods )
				{
					if( method.Name == nameof( IPersistsData.GetData ) )
					{
						ParameterInfo retParam = method.ReturnParameter;
						ParameterInfo[] methodParams = method.GetParameters();

						if( retParam.ParameterType == typeof( SerializedData )
						 && methodParams.Length == 2
						 && methodParams[1].ParameterType == typeof( IReverseReferenceMap ) )
						{
							_getDatas.Add( methodParams[0].ParameterType, method );
						}
					}
					if( method.Name == nameof( IPersistsData.SetData ) )
					{
						ParameterInfo retParam = method.ReturnParameter;
						ParameterInfo[] methodParams = method.GetParameters();

						if( retParam.ParameterType == typeof( void )
						 && methodParams.Length == 3
						 && methodParams[1].ParameterType == typeof( IReverseReferenceMap )
						 && methodParams[2].ParameterType == typeof( SerializedData ) )
						{
							_setDatas.Add( methodParams[0].ParameterType, method );
						}
					}
				}
			}
		}

		/// <summary>
		/// Creates an object instance from the serialized data (without the internal state). To set the state, call <see cref="SetData(object, IReverseReferenceMap)"/>.
		/// </summary>
		public static object ToInstance( SerializedObject data, IForwardReferenceMap l )
		{
			Type type = data[KeyNames.TYPE].ToType();
			Guid id = data[KeyNames.ID].ToGuid();

			object obj = null;
			if( _cache.TryGetClosest( type, out var factoryFunc ) )
			{
				obj = factoryFunc.ToInstanceFuncs.Invoke( data, l );
			}
			else
			{
				obj = Activator.CreateInstance( type );
			}

			l.SetObj( id, obj );

			if( obj is IAutoPersistsObjects )
			{
				if( !_datapersistentMembers.TryGetValue( type, out var array ) )
				{
					array = CacheType( type ).refT;
				}

				foreach( var field in array.fields ) // for each field, save the field, and whatever it owns
				{
					if( data.TryGetValue( field.attr.Key, out var fieldData ) )
					{
						object fieldValue = ToInstance( (SerializedObject)fieldData, l );
						field.f.SetValue( obj, fieldValue );
					}
				}
				foreach( var property in array.properties )
				{
					if( data.TryGetValue( property.attr.Key, out var propertyData ) )
					{
						object fieldValue = ToInstance( (SerializedObject)propertyData, l );
						property.p.SetValue( obj, fieldValue );
					}
				}
			}

			if( obj is IPersistsObjects p )
			{
				p.SetObjects( l, data ); // this can override auto-serialized members
			}

			return obj;
		}

		/// <summary>
		/// Returns the serialized instance of the object (without the internal state). To get the state, call <see cref="GetData(object, IReverseReferenceMap)"/>.
		/// </summary>
		public static SerializedObject ToSerialized( this object obj, IReverseReferenceMap s )
		{
			Type type = obj.GetType();

			SerializedObject rootSO = new SerializedObject()
			{
				{ KeyNames.ID, s.GetID( obj ).GetData() },
				{ KeyNames.TYPE, type.GetData() }
			};

			if( obj is IAutoPersistsObjects )
			{
				if( !_referencepersistentMembers.TryGetValue( type, out var array ) )
				{
					array = CacheType( type ).refT;
				}

				foreach( var field in array.fields ) // for each field, save the field, and whatever it owns
				{
					object fieldValue = field.f.GetValue( obj );

					SerializedObject so = ToSerialized( fieldValue, s );
					rootSO.Add( field.attr.Key, so );
				}
				foreach( var property in array.properties )
				{
					object propertyValue = property.p.GetValue( obj );

					SerializedObject so = ToSerialized( propertyValue, s );
					rootSO.Add( property.attr.Key, so );
				}
			}

			if( obj is IPersistsObjects p )
			{
				SerializedObject ownsMap = p.GetObjects( s ); // this can override auto-serialized members

				foreach( var kvp in ownsMap )
				{
					rootSO.Add( kvp.Key, kvp.Value );
				}
			}

			return rootSO;
		}

		//
		//	DATA
		//

		// TODO - For get/setdata of derived objects, we actually want to call the get/setdata of every base type as well. Including if that type is not "ours".

		// The use case is to enable serialization of base fields/properties without having access to the base type (e.g. for `enabled` from UnityEngine.Behaviour).

		public static SerializedData GetData( this object obj, IReverseReferenceMap s )
		{
			if( obj is IAutoPersistsData )
			{
				Type type = obj.GetType();

				if( !_datapersistentMembers.TryGetValue( type, out var array ) )
				{
					array = CacheType( type ).refT;
				}

				SerializedObject rootSO = new SerializedObject();

				foreach( var field in array.fields ) // for each field, save the field, and whatever it owns
				{
					object fieldValue = field.f.GetValue( obj );

					SerializedData so = GetData( fieldValue, s );
					rootSO.Add( field.attr.Key, so );
				}
				foreach( var property in array.properties )
				{
					object propertyValue = property.p.GetValue( obj );

					SerializedData so = GetData( propertyValue, s );
					rootSO.Add( property.attr.Key, so );
				}

				return rootSO;
			}

			switch( obj )
			{
				case IPersistsData o:
					return o.GetData( s );
				case Component o:
					return Persistent_Component.GetData( o, s );
				default:
					if( _getDatas.TryGetValue( obj.GetType(), out var method ) )
					{
						return (SerializedData)method.Invoke( obj, new object[] { s } );
					}
					break;
			}
			return null;
		}

		public static void SetData( this object obj, IForwardReferenceMap l, SerializedData data )
		{
			if( obj is IAutoPersistsData )
			{
				Type type = obj.GetType();

				if( !_datapersistentMembers.TryGetValue( type, out var array ) )
				{
					array = CacheType( type ).refT;
				}

				foreach( var field in array.fields ) // for each field, save the field, and whatever it owns
				{
					if( data.TryGetValue( field.attr.Key, out var fieldData ) )
					{
						object fieldValue = field.f.GetValue( obj );
						if( fieldValue != null )
						{
							SetData( fieldValue, l, fieldData );
						}
					}
				}
				foreach( var property in array.properties )
				{
					if( data.TryGetValue( property.attr.Key, out var propertyData ) )
					{
						object propertyValue = property.p.GetValue( obj );
						if( propertyValue != null )
						{
							SetData( propertyValue, l, propertyData );
						}
					}
				}
			}

			switch( obj )
			{
				case IPersistsData o:
					o.SetData( l, data ); break;
				case Component o:
					Persistent_Component.SetData( o, l, data ); break;
				default:
					if( _setDatas.TryGetValue( obj.GetType(), out var method ) )
					{
						method.Invoke( obj, new object[] { l, data } );
					}
					break;
			}
		}
	}
}