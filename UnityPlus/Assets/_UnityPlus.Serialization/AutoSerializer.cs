using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace UnityPlus.Serialization
{
	[AttributeUsage( AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false )]
	public sealed class PersistDataAttribute : Attribute
	{
		// must be IPersistsData or have a suitable method.
		public string Key { get; set; }

		public PersistDataAttribute( string key )
		{
			this.Key = key;
		}
	}

	[AttributeUsage( AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false )]
	public sealed class PersistReferenceAttribute : Attribute
	{
		// must have a default constructor.
		public string Key { get; set; }

		public PersistReferenceAttribute( string key )
		{
			this.Key = key;
		}
	}

	public static class AutoSerializer
	{
		private static readonly Dictionary<Type, (PropertyInfo[] fields, FieldInfo[] properties)> _persistentMembers = new();

		public static SerializedData GetObjects( object obj, IReverseReferenceMap s )
		{
			// those additional objects mean that an object basically owns more objects.

			SerializedArray arr = new SerializedArray();

			if( _persistentMembers.TryGetValue( obj.GetType(), out var array ) )
			{
				foreach( var field in array.fields )
				{
					object value = field.GetValue( obj );
					arr.Add( Strategies.StratUtils.WriteObjectInstance( s, value ) );

					// recursively try calling on each serialized object.
					// needs something that'll have methods to serialize objects already prepared and cached. probably in a dict, with some like IPersistent hardcoded to serialize self.
				}
			}
		}

		public static void SetObjects( object obj, IForwardReferenceMap l, SerializedData data )
		{

		}

		public static SerializedData GetData( object obj, IReverseReferenceMap s )
		{

		}

		public static void SetData( object obj, IForwardReferenceMap l, SerializedData data )
		{

		}
	}
}