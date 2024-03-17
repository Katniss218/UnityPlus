using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace UnityPlus.Serialization
{
	public static class Persistent_Vector2Int
	{
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static SerializedData GetData( this Vector2Int v )
		{
			return new SerializedArray() { (SerializedPrimitive)v.x, (SerializedPrimitive)v.y };
		}
		
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static void SetData( this ref Vector2Int v, SerializedData data )
		{
			v.x = (int)data[0];
			v.y = (int)data[1];
		}
		
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static Vector2Int Vector2Int( this SerializedData data ) 
		{
            return new Vector2Int( (int)data[0], (int)data[1] );
		}
	}
}