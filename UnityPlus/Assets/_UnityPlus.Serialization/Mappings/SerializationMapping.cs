using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Profiling;

namespace UnityPlus.Serialization
{
    public class EmptySerializationMapping : SerializationMapping
    {
        public override SerializedData Save( object obj, IReverseReferenceMap s )
        {
            return null;
        }

        public override object Load( SerializedData data, IForwardReferenceMap l )
        {
            return null;
        }

        public override void LoadReferences( object obj, SerializedData data, IForwardReferenceMap l )
        {

        }
    }

    public abstract class SerializationMapping
    {
        private struct Entry
        {
            public SerializationMapping mapping;
            public MethodInfo method;
            public bool isReady;
        }

        private static readonly TypeMap<Entry> _mappings = new();

        private static bool _isInitialized = false;

        public abstract SerializedData Save( object obj, IReverseReferenceMap s );
        public abstract object Load( SerializedData data, IForwardReferenceMap l );
        public abstract void LoadReferences( object obj, SerializedData data, IForwardReferenceMap l );

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
#warning TODO - search and add generic mappings. (if method is generic)
                    // this requires lazy-loading, because we have to have the generic parameters to invoke a generic method to get its mapping.

                    SerializationMappingProviderAttribute attr = method.GetCustomAttribute<SerializationMappingProviderAttribute>();
                    if( attr == null )
                        continue;

                    if( method.ReturnParameter.ParameterType != typeof( SerializationMapping ) )
                        continue;

                    if( method.GetParameters().Length != 0 )
                        continue;

                    var targetType = attr.TargetType;
                    //var mapping = (SerializationMapping)method.Invoke( null, null );

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

        public static SerializationMapping GetMappingFor<TMember>( TMember memberObj )
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
            return new EmptySerializationMapping();
        }

        public static SerializationMapping GetMappingFor<TMember>( Type objType )
        {
            if( !_isInitialized )
                Initialize();

            if( typeof( TMember ).IsAssignableFrom( objType ) ) // This doesn't appear to be much of a slow point.
            {
                if( _mappings.TryGetClosest( objType, out var entry ) )
                {
                    if( !entry.isReady )
                    {
                        var entry2 = MakeReady( entry, objType );
                        return entry2.mapping;
                    }

                    return entry.mapping;
                }
                return new EmptySerializationMapping();
            }

            throw new ArgumentException( $"Object type must be a subtype of member type, was {typeof( TMember ).Name} <==/== {objType.Name}.", nameof( objType ) );
        }
    }
}