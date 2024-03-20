using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace UnityPlus.Serialization
{
	public static class Persistent_object
	{
		private static readonly Dictionary<Type, ((FieldInfo f, PersistAttribute attr)[] fields, (PropertyInfo p, PersistAttribute attr)[] properties)> _referencepersistentMembers = new();
		private static readonly Dictionary<Type, ((FieldInfo f, PersistAttribute attr)[] fields, (PropertyInfo p, PersistAttribute attr)[] properties)> _datapersistentMembers = new();

		private static (((FieldInfo f, PersistAttribute attr)[] fields, (PropertyInfo p, PersistAttribute attr)[] properties) refT, ((FieldInfo f, PersistAttribute attr)[] fields, (PropertyInfo p, PersistAttribute attr)[] properties) dataT) CacheType( Type type )
		{
			List<(FieldInfo f, PersistAttribute attr)> finalDataFields = new();
			List<(FieldInfo f, PersistAttribute attr)> finalReferenceFields = new();
			List<(PropertyInfo p, PersistAttribute attr)> finalDataProperties = new();
			List<(PropertyInfo p, PersistAttribute attr)> finalReferenceProperties = new();

			FieldInfo[] fields = type.GetFields( BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy );
			foreach( var field in fields )
			{
				PersistAttribute attr = field.GetCustomAttribute<PersistAttribute>();
				if( attr != null )
				{
					if( attr.PersistsReference )
						finalReferenceFields.Add( (field, attr) );
					if( attr.PersistsData )
						finalDataFields.Add( (field, attr) );
				}
			}

			PropertyInfo[] properties = type.GetProperties( BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy );
			foreach( var property in properties )
			{
				PersistAttribute attr = property.GetCustomAttribute<PersistAttribute>();
				if( attr != null )
				{
					if( attr.PersistsReference )
						finalReferenceProperties.Add( (property, attr) );
					if( attr.PersistsData )
						finalDataProperties.Add( (property, attr) );
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

#warning TODO - replace "serialization strategies" by these factories. i.e. a hierarchy strategy is just a method that can create a gameobject instance when it sees a type of gameobject.

		// asset strategy sees something that tells it that it's an asset.
		// - this allows both to work simultaneously.


		// gameobjects are not auto-persistent, so the factory will create the entire hierarchy (if applicable), or spawn a prefab (if marked as prefab)

		// This will replace the explicit hierarchy and asset gameobjects with a single "load scene from file" type thing.


		// backwards compatibility of save files can be done with a separate system, which takes in the version of the file and updates the file itself before it is loaded.

		// when gameobject factory creates children, it should call the factory recursively, on those children.


		private static readonly Dictionary<Type, Func<Type, SerializedObject, object>> _factoryCache = new(); // pass in the target type, and creation data.

		public static void RegisterFactory<T>( Func<Type, SerializedObject, object> factory )
		{
			Type keyType = typeof(T);
			_factoryCache[keyType] = factory;
		}

		public static Func<Type, SerializedObject, object> GetMostCompatibleFactory( Type type )
		{
			if( !_factoryCache.Any() )
			{
				return null;
			}

			Type targetType = type;

			_factoryCache.TryGetValue( targetType, out var factory );

			if( factory == null )
			{
				if( type.IsGenericType ) // if there is no entry for a specific generic type, get the entry for the unspecified generic type (if any).
				{
					targetType = type.GetGenericTypeDefinition();

					_factoryCache.TryGetValue( targetType, out factory );

					// maybe use parent types here as well.
					// what about interfaces?
				}
				else // drawer for the base type of the type we want.
				{
					targetType = type.BaseType;

					while( true )
					{
						_factoryCache.TryGetValue( targetType, out factory );

						if( factory != null )
						{
							break;
						}
						if( targetType.BaseType == null )
						{
							break;
						}

						targetType = targetType.BaseType;
					}
				}
			}

			_factoryCache[type] = factory;

			return factory;
		}

		/// <summary>
		/// Returns the serialized instance of the object (without the internal state). To get the state, call <see cref="GetData(object, IReverseReferenceMap)"/>.
		/// </summary>
		public static SerializedObject GetObjects( this object obj, IReverseReferenceMap s )
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

					SerializedObject so = GetObjects( fieldValue, s );
					rootSO.Add( field.attr.Key, so );
				}
				foreach( var property in array.properties )
				{
					object propertyValue = property.p.GetValue( obj );

					SerializedObject so = GetObjects( propertyValue, s );
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

		/// <summary>
		/// Creates an object instance from the serialized data (without the internal state). To set the state, call <see cref="SetData(object, IReverseReferenceMap)"/>.
		/// </summary>
		public static object SetObjects( IForwardReferenceMap l, SerializedObject data )
		{
			Type type = data[KeyNames.TYPE].ToType();

			object obj = null;
			if( _factoryCache.TryGetValue( type, out var factoryFunc ) )
			{
				obj = factoryFunc.Invoke( type, data );
			}
			else
			{
				obj = Activator.CreateInstance( type );
			}

			l.SetObj( data[KeyNames.ID].ToGuid(), obj );

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
						object fieldValue = SetObjects( l, (SerializedObject)fieldData );
						field.f.SetValue( obj, fieldValue );
					}
				}
				foreach( var property in array.properties )
				{
					if( data.TryGetValue( property.attr.Key, out var propertyData ) )
					{
						object fieldValue = SetObjects( l, (SerializedObject)propertyData );
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

		//
		//	DATA
		//

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

					SerializedObject so = GetObjects( fieldValue, s );
					rootSO.Add( field.attr.Key, so );
				}
				foreach( var property in array.properties )
				{
					object propertyValue = property.p.GetValue( obj );

					SerializedObject so = GetObjects( propertyValue, s );
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
						object fieldValue = SetObjects( l, (SerializedObject)fieldData );
						field.f.SetValue( obj, fieldValue );
					}
				}
				foreach( var property in array.properties )
				{
					if( data.TryGetValue( property.attr.Key, out var propertyData ) )
					{
						object fieldValue = SetObjects( l, (SerializedObject)propertyData );
						property.p.SetValue( obj, fieldValue );
					}
				}
			}

			switch( obj )
			{
				case IPersistsData o:
					o.SetData( l, data ); break;
				case Component o:
					Persistent_Component.SetData( o, l, data ); break;
			}
		}
	}
}