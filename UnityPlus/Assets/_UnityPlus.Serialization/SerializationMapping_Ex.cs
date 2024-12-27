using System.Runtime.CompilerServices;
using UnityEngine;

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
        public static bool SafeSave<TMember>( this SerializationMapping mapping, TMember obj, ref SerializedData data, ISaver s )
        {
            if( mapping == null )
                return false;

            //SerializedData data = null; // delete this and move back to just returning serializeddata?

            return mapping.Save<TMember>( obj, ref data, s );
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

            return mapping.Load<TMember>( ref obj, data, l );
        }
    }
}