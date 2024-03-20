using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace UnityPlus.Serialization
{
	public static class Persistent_DateTime
	{
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static SerializedPrimitive GetData( this DateTime dateTime )
		{
			// DateTime should be saved as an ISO-8601 string.
			return (SerializedPrimitive)dateTime.ToString( "s", CultureInfo.InvariantCulture );
		}
		
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static void SetData( this ref DateTime dateTime, SerializedData data )
		{
			dateTime = DateTime.Parse( (string)data, CultureInfo.InvariantCulture );
		}
		
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static DateTime ToDateTime( this SerializedData data ) 
		{
			return DateTime.Parse( (string)data, CultureInfo.InvariantCulture );
		}
	}
}