using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace UnityPlus.Serialization
{
    public delegate void LoadAction( ref object obj, SerializedData data, ILoader l );
    public delegate void LoadReferencesAction( ref object obj, SerializedData data, ILoader l );

    public delegate void LoadAction<TSource>( ref TSource obj, SerializedData data, ILoader l );
    public delegate void LoadReferencesAction<TSource>( ref TSource obj, SerializedData data, ILoader l );

    public static class MappingHelper
    {
        // DoSave is the same everywhere.

#warning TODO - these can be moved into the SerializationMapping
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static void DoPopulate<T>( SerializationMapping mapping, ref T obj, SerializedData data, ILoader l )
        {
            object obj2;
            switch( mapping.SerializationStyle )
            {
                default:
                    return;
                case SerializationStyle.PrimitiveStruct:
                    obj2 = mapping.Instantiate( data, l );
                    break;
                case SerializationStyle.NonPrimitive:
                    obj2 = obj; // Don't instantiate when populating, object should already be created.
                    mapping.Load( ref obj2, data, l );
                    break;
            }
            obj = (T)obj2;
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static void DoLoad<T>( SerializationMapping mapping, ref T obj, SerializedData data, ILoader l )
        {
            object obj2;
            switch( mapping.SerializationStyle )
            {
                default:
                    return;
                case SerializationStyle.PrimitiveStruct:
                    obj2 = mapping.Instantiate( data, l );
                    break;
                case SerializationStyle.NonPrimitive:
                    obj2 = mapping.Instantiate( data, l );
                    mapping.Load( ref obj2, data, l );
                    break;
            }
            obj = (T)obj2;
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static void DoLoadReferences<T>( SerializationMapping mapping, ref T obj, SerializedData data, ILoader l )
        {
            object obj2 = obj;
            switch( mapping.SerializationStyle )
            {
                default:
                    return;
                case SerializationStyle.PrimitiveObject:
                    obj2 = mapping.Instantiate( data, l );
                    break;
                case SerializationStyle.NonPrimitive:
                    mapping.LoadReferences( ref obj2, data, l );
                    break;
            }
            obj = (T)obj2;
        }
    }
}