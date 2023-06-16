using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnityEngine.Serialization.Json
{
    public class JsonStringWriter
    {
        public void WriteJson( SerializedObject obj, StringBuilder sb )
        {
            sb.Append( '{' );

            bool seen = false;
            foreach( var child in obj )
            {
                if( seen )
                {
                    sb.Append( ':' );
                }
                else
                {
                    seen = true;
                }

                var str = $"\"{child.Key}\":";

                sb.Append( str );

                WriteJson( child.Value, sb );
            }

            sb.Append( '}' );
        }

        public void WriteJson( SerializedArray obj, StringBuilder sb )
        {
            sb.Append( '[' );

            bool seen = false;
            foreach( var child in obj )
            {
                if( seen )
                {
                    sb.Append( ',' );
                }
                else
                {
                    seen = true;
                }
                WriteJson( child, sb );
            }

            sb.Append( ']' );
        }

        public void WriteJson( SerializedValue value, StringBuilder sb )
        {
            if( value == null )
            {
                sb.Append( "null" );
                return;
            }

            string s = null;
            switch( value._valueType )
            {
                case SerializedValue.DataType.Boolean:
                    s = value._value.boolean ? "true" : "false"; break;
                case SerializedValue.DataType.Int:
                    s = value._value.@int.ToString( CultureInfo.InvariantCulture ); break;
                case SerializedValue.DataType.UInt:
                    s = value._value.@uint.ToString( CultureInfo.InvariantCulture ); break;
                case SerializedValue.DataType.Float:
                    s = value._value.@float.ToString( CultureInfo.InvariantCulture ); break;
                case SerializedValue.DataType.Decimal:
                    s = value._value.@decimal.ToString( CultureInfo.InvariantCulture ); break;
                case SerializedValue.DataType.String:
                    s = $"\"{(string)value._value.obj}\""; break;
                case SerializedValue.DataType.Object:
                    WriteJson( (SerializedObject)value._value.obj, sb ); return;
                case SerializedValue.DataType.Array:
                    WriteJson( (SerializedArray)value._value.obj, sb ); return;
            }

            sb.Append( s );
        }
    }
}