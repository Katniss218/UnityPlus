using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace UnityPlus.Serialization
{
    public static class SerializationMappingRegistry
    {
        private struct Entry
        {
            public SerializationMapping mapping;
            public MethodInfo method;
            public bool isReady;
        }

        private static readonly TypeMap<Entry> _mappings = new();

        private static bool _isInitialized = false;

        private static IEnumerable<Type> GetTypes()
        {
            return AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany( a => a.GetTypes() );
        }

        private static void Initialize()
        {
            foreach( var containingType in GetTypes() )
            {
                MethodInfo[] methods = containingType.GetMethods( BindingFlags.Public | BindingFlags.Static );
                foreach( var method in methods )
                {
                    SerializationMappingProviderAttribute attr = method.GetCustomAttribute<SerializationMappingProviderAttribute>();
                    if( attr == null )
                        continue;

                    if( method.ReturnParameter.ParameterType != typeof( SerializationMapping ) )
                        continue;

                    if( method.GetParameters().Length != 0 )
                        continue;

                    // Find every method that returns a mapping, and cache it.
                    // In case the mapping (and method) is generic, the call is deferred to when the type parameters are known.

                    var targetType = attr.TargetType;

                    var entry = new Entry()
                    {
                        mapping = null,
                        method = method,
                        isReady = false
                    };

                    _mappings.Set( targetType, entry );
                }
            }

            _isInitialized = true;
        }

        private static Entry MakeReady( Entry entry, Type objType )
        {
            var method = entry.method;

            if( objType.IsArray )
            {
                method = method.MakeGenericMethod( objType.GetElementType() );
            }
            else if( objType.IsEnum )
            {
                method = method.MakeGenericMethod( objType );
            }
            else if( method.ContainsGenericParameters )
            {
                method = method.MakeGenericMethod( objType.GetGenericArguments() );
            }

            var mapping = (SerializationMapping)method.Invoke( null, null );
            entry.isReady = true;
            entry.mapping = mapping;

            _mappings.Set( objType, entry );
            return entry;
        }

        /// <summary>
        /// Retrieves a serialization mapping for the given member type.
        /// </summary>
        /// <typeparam name="TMember">The type of the member ("variable") that the object is/will be assigned to.</typeparam>
        /// <param name="memberObj">The object.</param>
        /// <returns>The correct serialization mapping for the given object.</returns>
        internal static SerializationMapping GetMappingOrEmpty( Type memberType )
        {
            if( !_isInitialized )
                Initialize();

            if( _mappings.TryGetClosest( memberType, out var entry ) )
            {
                if( !entry.isReady )
                {
                    var entry2 = MakeReady( entry, memberType );
                    return entry2.mapping;
                }

                return entry.mapping;
            }

            return SerializationMapping.Empty( memberType );
        }

        /// <summary>
        /// Retrieves a serialization mapping for the given member type.
        /// </summary>
        /// <typeparam name="TMember">The type of the member ("variable") that the object is/will be assigned to.</typeparam>
        /// <param name="memberObj">The object.</param>
        /// <returns>The correct serialization mapping for the given object.</returns>
        public static SerializationMapping GetMappingOrDefault<TMember>( TMember memberObj )
        {
            if( !_isInitialized )
                Initialize();

            var objType = memberObj.GetType();
            if( _mappings.TryGetClosest( objType, out var entry ) )
            {
                if( !entry.isReady )
                {
                    var entry2 = MakeReady( entry, objType );
                    return entry2.mapping;
                }

                return entry.mapping;
            }

            return SerializationMapping.Empty<TMember>();
        }

        public static SerializationMapping GetMappingOrDefault<TMember>( Type memberType )
        {
            if( !_isInitialized )
                Initialize();

            if( typeof( TMember ).IsAssignableFrom( memberType ) ) // This doesn't appear to be much of a slow point.
            {
                if( _mappings.TryGetClosest( memberType, out var entry ) )
                {
                    if( !entry.isReady )
                    {
                        var entry2 = MakeReady( entry, memberType );
                        return entry2.mapping;
                    }

                    return entry.mapping;
                }
            }

            return SerializationMapping.Empty<TMember>();
        }
    }
}