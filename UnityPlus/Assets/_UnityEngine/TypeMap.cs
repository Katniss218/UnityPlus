using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;

namespace UnityEngine
{
	public class TypeMap<T>
	{
		private readonly Dictionary<Type, T> _map = new(); // pass in the target type, and creation data.

		public TypeMap()
		{

		}

		public void Set<TType>( T value )
		{
			Set( typeof( TType ), value );
		}

		public void Set( Type type, T value )
		{
			_map[type] = value;
		}

		public T GetOrDefault( Type type )
		{
			if( type == null )
			{
				throw new ArgumentNullException( nameof( type ) );
			}
			if( type.IsInterface )
			{
				throw new ArgumentException( $"The type to check can't be an interface.", nameof( type ) );
			}

			if( _map.TryGetValue( type, out var value ) )
			{
				return value;
			}

			_map[type] = value;

			return value;
		}

		public T GetClosestOrDefault( Type type )
		{
			if( type == null )
			{
				throw new ArgumentNullException( nameof( type ) );
			}
			if( type.IsInterface )
			{
				throw new ArgumentException( $"The type to check can't be an interface.", nameof( type ) );
			}

			if( _map.Count == 0 )
			{
				return default;
			}

			Type currentTypeToCheck = type;
			T value;

			while( !_map.TryGetValue( currentTypeToCheck, out value ) )
			{
				if( currentTypeToCheck.IsGenericType && currentTypeToCheck.IsConstructedGenericType )
				{
					if( _map.TryGetValue( currentTypeToCheck.GetGenericTypeDefinition(), out value ) )
					{
						return value;
					}
				}

				currentTypeToCheck = currentTypeToCheck.BaseType;
				if( currentTypeToCheck == null )
				{
					return default;
				}
			}

			_map[type] = value;

			return value;
		}
	}
}