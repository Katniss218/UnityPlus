using System;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace UnityPlus.Serialization
{
    public static class MappingHelper
    {
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static Type GetTypeOf<T>( SerializedData data )
        {
            if( data == null )
                return typeof( T );

            if( data.TryGetValue( KeyNames.TYPE, out var type ) )
                return type.DeserializeType();

            return typeof( T );
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static SerializationMapping GetMapping_Load<T>( int context, Type memberType, SerializedData data, ILoader l )
        {
            if( data == null )
                return SerializationMappingRegistry.GetMapping<T>( context, memberType );

            if( l.MappingCache.TryGetValue( data, out var mapping ) )
                return mapping;

            mapping = SerializationMappingRegistry.GetMapping<T>( context, memberType );

            l.MappingCache[data] = mapping;

            return mapping;
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static SerializationMapping GetMapping_LoadReferences<T>( int context, T member, SerializedData data, ILoader l )
        {
            if( data == null )
                return SerializationMappingRegistry.GetMapping<T>( context, member );

            if( l.MappingCache.TryGetValue( data, out var mapping ) )
                return mapping;

            return SerializationMappingRegistry.GetMapping<T>( context, member );
        }

        // TODO - these could be moved into the SerializationMapping

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static SerializedData DoSave<T>( SerializationMapping mapping, T obj, ISaver s )
        {
            if( mapping == null )
                return null;

            switch( mapping.SerializationStyle )
            {
                default:
                    return null;
                case SerializationStyle.PrimitiveObject:
                case SerializationStyle.PrimitiveStruct:
                case SerializationStyle.NonPrimitive:
                    return mapping.Save( obj, s );
            }
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static bool DoPopulate<T>( SerializationMapping mapping, ref T obj, SerializedData data, ILoader l )
        {
            if( mapping == null )
                return false;

            switch( mapping.SerializationStyle )
            {
                default:
                    return false;
                case SerializationStyle.PrimitiveStruct:
                    obj = (T)mapping.Instantiate( data, l );
                    return true;
                case SerializationStyle.NonPrimitive:
                    object obj2 = obj; // Don't instantiate when populating, object should already be created.
                    mapping.Load( ref obj2, data, l );
                    obj = (T)obj2;
                    return true;
            }
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static bool DoLoad<T>( SerializationMapping mapping, ref T obj, SerializedData data, ILoader l )
        {
            if( mapping == null )
                return false;

            switch( mapping.SerializationStyle )
            {
                default:
                    return false;
                case SerializationStyle.PrimitiveStruct:
                    obj = (T)mapping.Instantiate( data, l );
                    return true;
                case SerializationStyle.NonPrimitive:
                    object obj2 = mapping.Instantiate( data, l );
                    mapping.Load( ref obj2, data, l );
                    obj = (T)obj2;
                    return true;
            }
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static bool DoLoadReferences<T>( SerializationMapping mapping, ref T obj, SerializedData data, ILoader l )
        {
            if( mapping == null )
                return false;

            switch( mapping.SerializationStyle )
            {
                default:
                    return false;
                case SerializationStyle.PrimitiveObject:
                    obj = (T)mapping.Instantiate( data, l );
                    return true;
                case SerializationStyle.NonPrimitive:
                    object obj2 = obj;
                    mapping.LoadReferences( ref obj2, data, l );
                    obj = (T)obj2;
                    return true;
            }
        }
    }
}