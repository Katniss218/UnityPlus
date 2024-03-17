using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using UnityPlus.Serialization;

namespace UnityPlus.Serialization
{
	public static class Persistent_Boolean
	{
		// TODO - add equivalent for all other types.

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static SerializedData GetData( this bool value )
		{
			return (SerializedPrimitive)value;
		}
		
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static void SetData( this ref bool value, SerializedData data )
		{
			value = (bool)data;
		}
		
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static bool ToBoolean( this SerializedData data ) 
		{
            return (bool)data;
		}
	}
}