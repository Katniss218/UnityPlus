using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace UnityPlus.Serialization
{
	public static class PersistentQuaternion
	{
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static SerializedData GetData( this Quaternion q )
		{
			return new SerializedArray() { (SerializedPrimitive)q.x, (SerializedPrimitive)q.y, (SerializedPrimitive)q.z, (SerializedPrimitive)q.w };
		}
		
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static void SetData( this ref Quaternion q, SerializedData data )
		{
			q.x = (float)data[0];
			q.y = (float)data[1];
			q.z = (float)data[2];
			q.w = (float)data[3];
		}
		
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static Quaternion ToQuaternion( this SerializedData data ) 
		{
            return new Quaternion( (float)data[0], (float)data[1], (float)data[2], (float)data[3] );
		}
	}
}