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
            object val = value?.GetValue();
            string str = null;

            if( val == null )
                str = "null";
            else if( val is bool b )
                str = b ? "true" : "false";
            else if( val is byte )
                str = ((byte)val).ToString( CultureInfo.InvariantCulture );
            else if( val is short )
                str = ((short)val).ToString( CultureInfo.InvariantCulture );
            else if( val is int )
                str = ((int)val).ToString( CultureInfo.InvariantCulture );
            else if( val is long )
                str = ((long)val).ToString( CultureInfo.InvariantCulture );
            else if( val is float )
                str = ((float)val).ToString( CultureInfo.InvariantCulture );
            else if( val is double )
                str = ((double)val).ToString( CultureInfo.InvariantCulture );
            else if( val is decimal )
                str = ((decimal)val).ToString( CultureInfo.InvariantCulture );
            else if( val is string )
                str = $"\"{val}\"";
            else if( val is SerializedObject obj )
                obj.WriteJson( stream );
            else if( val is SerializedArray list )
                list.WriteJson( stream );

            stream.Write( enc.GetBytes( str ), 0, str.Length );
        }
    }
}
