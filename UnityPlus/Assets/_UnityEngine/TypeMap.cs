﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnityEngine
{
    /// <summary>
    /// Maps a type to a value. Allows hierarchical searching.
    /// </summary>
    public class TypeMap<T>
    {
        private readonly Dictionary<Type, T> _map = new(); // pass in the target type, and creation data.

        public TypeMap()
        {

        }

        public TypeMap( T defaultValue )
        {
            _map[typeof( object )] = defaultValue;
        }

        public bool TryGet( Type type, out T value )
        {
            if( type == null )
            {
                throw new ArgumentNullException( nameof( type ) );
            }

            return _map.TryGetValue( type, out value );
        }


        /// <summary>
        /// Gets the value for the given type (if exists), or default.
        /// </summary>
        /// <param name="type">The type to match.</param>
        /// <returns>The value corresponding to the given type.</returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException"></exception>
        public T GetOrDefault( Type type )
        {
            if( type == null )
            {
                throw new ArgumentNullException( nameof( type ) );
            }

            return _map.TryGetValue( type, out var value ) ? value : default;
        }

        public bool TryGetClosest( Type type, out T value )
        {
            if( type == null )
            {
                throw new ArgumentNullException( nameof( type ) );
            }

            if( _map.Count == 0 )
            {
                value = default;
                return false;
            }

            Type currentTypeToCheck = type;

            while( !_map.TryGetValue( currentTypeToCheck, out value ) )
            {
                if( currentTypeToCheck.IsGenericType && currentTypeToCheck.IsConstructedGenericType )
                {
                    if( _map.TryGetValue( currentTypeToCheck.GetGenericTypeDefinition(), out value ) )
                    {
                        return true;
                    }
                }

                currentTypeToCheck = currentTypeToCheck.BaseType;
                if( currentTypeToCheck == null )
                {
                    return false;
                }
            }

            return true;
        }

        public T GetClosestOrDefault( Type type )
        {
            if( type == null )
            {
                throw new ArgumentNullException( nameof( type ) );
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

            return value;
        }

        /// <summary>
        /// Sets the value for the corresponding type.
        /// </summary>
        /// <typeparam name="TType">The type to set the value for.</typeparam>
        /// <param name="value">The value to set.</param>
        public void Set<TType>( T value )
        {
            Set( typeof( TType ), value );
        }

        /// <summary>
        /// Sets the value for the corresponding type.
        /// </summary>
        /// <param name="type">The type to set the value for.</param>
        /// <param name="value">The value to set.</param>
        public void Set( Type type, T value )
        {
            _map[type] = value;
        }

        public void Clear()
        {
            _map.Clear();
        }
    }
}