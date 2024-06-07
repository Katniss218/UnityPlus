using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace UnityPlus.Serialization.Mappings
{
    public static class MappingHelper
    {
        // DoSave is the same everywhere.

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static void DoLoad( SerializationMapping mapping, ref object a, SerializedData data, IForwardReferenceMap l )
        {
            switch( mapping.SerializationStyle )
            {
                default:
                    return;
                case SerializationStyle.PrimitiveStruct:
                    a = mapping.Instantiate( data, l );
                    break;
                case SerializationStyle.NonPrimitive:
                    mapping.Load( ref a, data, l );
                    break;
            }
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static void DoLoadReferences( SerializationMapping mapping, ref object a, SerializedData data, IForwardReferenceMap l )
        {
            switch( mapping.SerializationStyle )
            {
                default:
                    return;
                case SerializationStyle.PrimitiveObject:
                    a = mapping.Instantiate( data, l );
                    break;
                case SerializationStyle.NonPrimitive:
                    mapping.LoadReferences( ref a, data, l );
                    break;
            }
        }
    }
}