﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using UnityPlus.Serialization;

namespace UnityPlus.Serialization
{
	public static class Persistent_Int32
	{
		// TODO - add equivalent for all other types.

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static SerializedData GetData( this int value, IReverseReferenceMap s )
		{
			return (SerializedPrimitive)value;
		}
		
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static void SetData( this ref int value, IForwardReferenceMap l, SerializedData data )
		{
			value = (int)data;
		}
		
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static int ToInt32( this SerializedData data, IForwardReferenceMap l ) 
		{
            return (int)data;
		}
	}
}