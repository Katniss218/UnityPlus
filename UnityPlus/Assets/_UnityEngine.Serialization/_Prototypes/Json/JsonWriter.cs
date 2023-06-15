using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnityEngine.Serialization.Json
{
    public static class JsonWriter
    {
        static Encoding enc = Encoding.UTF8;

        public static void WriteJson( this SerializedObject obj, Stream stream )
        {
            stream.Write( enc.GetBytes( "{" ), 0, 1 );

            bool seen = false;
            foreach( var child in obj )
            {
                if( seen )
                {
                    stream.Write( enc.GetBytes( "," ), 0, 1 );
                }
                else
                {
                    seen = true;
                }

                var str = $"\"{child.Key}\":";

                stream.Write( enc.GetBytes( str ), 0, str.Length );

                child.Value.WriteJson( stream );
            }

            stream.Write( enc.GetBytes( "}" ), 0, 1 );
        }

        public static void WriteJson( this SerializedArray obj, Stream stream )
        {
            stream.Write( enc.GetBytes( "[" ), 0, 1 );

            bool seen = false;
            foreach( var child in obj )
            {
                if( seen )
                {
                    stream.Write( enc.GetBytes( "," ), 0, 1 );
                }
                else
                {
                    seen = true;
                }
                child.WriteJson( stream );
            }

            stream.Write( enc.GetBytes( "]" ), 0, 1 );
        }

        public static void WriteJson( this SerializedValue value, Stream stream )
        {
            if( value == null )
            {
                stream.Write( enc.GetBytes( "null" ), 0, "null".Length );
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
                    ((SerializedObject)value._value.obj).WriteJson( stream ); return;
                case SerializedValue.DataType.Array:
                    ((SerializedArray)value._value.obj).WriteJson( stream ); return;
            }

            stream.Write( enc.GetBytes( s ), 0, s.Length );
        }
    }
}
