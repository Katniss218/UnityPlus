using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using UnityPlus.Serialization;

namespace UnityPlus.Serialization
{
    public static class SerializationMapping_Ex
    {
        /// <summary>
        /// Use this method to invoke a mapping.
        /// </summary>
        /// <remarks>
        /// Doesn't require doing a null check on the mapping.
        /// </remarks>
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static SerializedData SafeSave<TMember>( this SerializationMapping mapping, TMember obj, ISaver s )
        {
            if( mapping == null )
                return null;

            SerializedData data = null;

            // Omit the header only for member types that are non-generic structs/sealed classes (non-generic and non-inheritable).
            if( obj != null && !((typeof( TMember ).IsValueType || typeof( TMember ).IsSealed) && !typeof( TMember ).IsGenericType) )
            {
                data = new SerializedObject();
                data[KeyNames.ID] = s.RefMap.GetID( obj ).SerializeGuid();
                data[KeyNames.TYPE] = obj.GetType().SerializeType();
            }

#warning TODO - depending on the existence of the header, the mapping itself should handle that.
            // Also, the header should be added by the mapping, because assets/refs don't need it.

            mapping.___passthroughSave<TMember>( obj, ref data, s );
            return data;
        }

        /// <summary>
        /// Use this method to invoke a mapping.
        /// </summary>
        /// <remarks>
        /// Doesn't require doing a null check on the mapping.
        /// </remarks>
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static bool SafePopulate<TMember>( this SerializationMapping mapping, ref TMember obj, SerializedData data, ILoader l )
        {
            if( mapping == null )
                return false;

            return mapping.___passthroughPopulate<TMember>( ref obj, data, l );
        }

        /// <summary>
        /// Use this method to invoke a mapping.
        /// </summary>
        /// <remarks>
        /// Doesn't require doing a null check on the mapping.
        /// </remarks>
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static bool SafeLoad<TMember>( this SerializationMapping mapping, ref TMember obj, SerializedData data, ILoader l )
        {
            if( mapping == null )
                return false;

            return mapping.___passthroughLoad<TMember>( ref obj, data, l );
        }

        /// <summary>
        /// Use this method to invoke a mapping.
        /// </summary>
        /// <remarks>
        /// Doesn't require doing a null check on the mapping.
        /// </remarks>
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static bool SafeLoadReferences<TMember>( this SerializationMapping mapping, ref TMember obj, SerializedData data, ILoader l )
        {
            if( mapping == null )
                return false;

            return mapping.___passthroughLoadReferences<TMember>( ref obj, data, l );
        }
    }
}