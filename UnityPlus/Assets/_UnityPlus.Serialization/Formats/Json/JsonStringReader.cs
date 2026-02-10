
using System;
using System.Collections.Generic;
using System.Globalization;

namespace UnityPlus.Serialization.Json
{
    public class JsonStringReader
    {
        private readonly ReadOnlyMemory<char> _json;
        private int _recursionDepth;
        private const int MaxDepth = 512;

        public JsonStringReader( string json )
        {
            _json = json.AsMemory();
        }

        public SerializedData Read()
        {
            var span = _json.Span;
            int index = 0;
            _recursionDepth = 0;

            SkipWhitespace( span, ref index );
            if( index >= span.Length ) return null;

            return ParseValue( span, ref index );
        }

        private SerializedData ParseValue( ReadOnlySpan<char> span, ref int index )
        {
            if( _recursionDepth > MaxDepth ) throw new FormatException( "JSON recursion depth limit exceeded." );

            SkipWhitespace( span, ref index );
            if( index >= span.Length ) return null;

            char c = span[index];

            switch( c )
            {
                case '{':
                    _recursionDepth++;
                    var obj = ParseObject( span, ref index );
                    _recursionDepth--;
                    return obj;
                case '[':
                    _recursionDepth++;
                    var arr = ParseArray( span, ref index );
                    _recursionDepth--;
                    return arr;
                case '"': return ParseString( span, ref index );
                case 't': // true
                    if( IsMatch( span, index, "true" ) ) { index += 4; return true; }
                    throw new FormatException( $"Invalid token at {index}" );
                case 'f': // false
                    if( IsMatch( span, index, "false" ) ) { index += 5; return false; }
                    throw new FormatException( $"Invalid token at {index}" );
                case 'n': // null
                    if( IsMatch( span, index, "null" ) ) { index += 4; return null; }
                    throw new FormatException( $"Invalid token at {index}" );
                default:
                    if( c == '-' || char.IsDigit( c ) ) return ParseNumber( span, ref index );
                    throw new FormatException( $"Invalid JSON token at index {index}: '{c}'" );
            }
        }

        private bool IsMatch( ReadOnlySpan<char> span, int index, string literal )
        {
            if( index + literal.Length > span.Length ) return false;
            for( int i = 0; i < literal.Length; i++ )
            {
                if( span[index + i] != literal[i] ) return false;
            }
            return true;
        }

        private SerializedObject ParseObject( ReadOnlySpan<char> span, ref int index )
        {
            index++; // Skip '{'
            var obj = new SerializedObject();
            bool first = true;

            while( true )
            {
                SkipWhitespace( span, ref index );
                if( index >= span.Length ) throw new FormatException( "Unexpected end of JSON input in Object" );

                if( span[index] == '}' )
                {
                    index++;
                    break;
                }

                if( !first )
                {
                    if( span[index] != ',' ) throw new FormatException( $"Expected ',' at index {index}" );
                    index++;
                    SkipWhitespace( span, ref index );
                }
                first = false;

                if( span[index] != '"' ) throw new FormatException( $"Expected property name at index {index}" );

                string key = ParseStringInternal( span, ref index );

                SkipWhitespace( span, ref index );
                if( index >= span.Length || span[index] != ':' ) throw new FormatException( $"Expected ':' at index {index}" );
                index++;

                SerializedData value = ParseValue( span, ref index );
                obj[key] = value;
            }
            return obj;
        }

        private SerializedArray ParseArray( ReadOnlySpan<char> span, ref int index )
        {
            index++; // Skip '['
            var arr = new SerializedArray();
            bool first = true;

            while( true )
            {
                SkipWhitespace( span, ref index );
                if( index >= span.Length ) throw new FormatException( "Unexpected end of JSON input in Array" );

                if( span[index] == ']' )
                {
                    index++;
                    break;
                }

                if( !first )
                {
                    if( span[index] != ',' ) throw new FormatException( $"Expected ',' at index {index}" );
                    index++;
                }
                first = false;

                SerializedData value = ParseValue( span, ref index );
                arr.Add( value );
            }
            return arr;
        }

        private SerializedPrimitive ParseString( ReadOnlySpan<char> span, ref int index )
        {
            return (SerializedPrimitive)ParseStringInternal( span, ref index );
        }

        private string ParseStringInternal( ReadOnlySpan<char> span, ref int index )
        {
            index++; // Skip opening quote
            int start = index;

            while( index < span.Length )
            {
                char c = span[index];
                if( c == '\\' )
                {
                    index += 2;
                    continue;
                }
                if( c == '"' )
                {
                    var content = span.Slice( start, index - start );
                    index++;
                    return JsonCommon.UnescapeString( content );
                }
                index++;
            }
            throw new FormatException( "Unterminated string literal" );
        }

        private SerializedPrimitive ParseNumber( ReadOnlySpan<char> span, ref int index )
        {
            int start = index;
            bool isFloat = false;

            // 1. Optional Minus
            if( span[index] == '-' )
            {
                index++;
                if( index >= span.Length ) throw new FormatException( "Invalid number format (lone minus)" );
            }

            // 2. Integer Part
            if( span[index] == '0' )
            {
                index++;
                // If starts with 0, next must NOT be a digit (unless it's . or e or end)
                if( index < span.Length && char.IsDigit( span[index] ) )
                    throw new FormatException( "Invalid number format (leading zero)" );
            }
            else if( char.IsDigit( span[index] ) )
            {
                index++;
                while( index < span.Length && char.IsDigit( span[index] ) ) index++;
            }
            else
            {
                throw new FormatException( $"Invalid number format at index {index}" );
            }

            // 3. Fraction Part
            if( index < span.Length && span[index] == '.' )
            {
                isFloat = true;
                index++;
                if( index >= span.Length || !char.IsDigit( span[index] ) ) throw new FormatException( "Invalid decimal format" );
                while( index < span.Length && char.IsDigit( span[index] ) ) index++;
            }

            // 4. Exponent Part
            if( index < span.Length && (span[index] == 'e' || span[index] == 'E') )
            {
                isFloat = true;
                index++;
                if( index < span.Length && (span[index] == '+' || span[index] == '-') ) index++;
                if( index >= span.Length || !char.IsDigit( span[index] ) ) throw new FormatException( "Invalid exponent format" );
                while( index < span.Length && char.IsDigit( span[index] ) ) index++;
            }

            var numberSpan = span.Slice( start, index - start );

#if NETSTANDARD2_1_OR_GREATER || UNITY_2021_3_OR_NEWER
            if( isFloat )
            {
                if( double.TryParse( numberSpan, NumberStyles.Float, CultureInfo.InvariantCulture, out double d ) )
                    return (SerializedPrimitive)d;
            }
            else
            {
                if( long.TryParse( numberSpan, NumberStyles.Integer, CultureInfo.InvariantCulture, out long l ) )
                    return (SerializedPrimitive)l;
            }
#else
            string numStr = numberSpan.ToString();
            if (isFloat)
            {
                if (double.TryParse(numStr, NumberStyles.Float, CultureInfo.InvariantCulture, out double d))
                    return (SerializedPrimitive)d;
            }
            else
            {
                if (long.TryParse(numStr, NumberStyles.Integer, CultureInfo.InvariantCulture, out long l))
                    return (SerializedPrimitive)l;
            }
#endif

            throw new FormatException( $"Could not parse number: {numberSpan.ToString()}" );
        }

        private void SkipWhitespace( ReadOnlySpan<char> span, ref int index )
        {
            while( index < span.Length && char.IsWhiteSpace( span[index] ) )
            {
                index++;
            }
        }
    }
}
