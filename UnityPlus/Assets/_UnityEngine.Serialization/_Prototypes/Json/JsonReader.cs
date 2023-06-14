using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace UnityEngine.Serialization.Json
{
    public class JsonReader
    {
        // make a custom parser + reader/writer.

        string _s;
        int _pos;

        char? _currentChar;
        char? _previousChar;

        public JsonReader( string json )
        {
            this._s = json;
            this._pos = 0;

            UpdateCharacterCache();
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        void Advance( int num = 1 )
        {
            _pos += num;

            UpdateCharacterCache();
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        void UpdateCharacterCache()
        {
            _previousChar = _currentChar;
            if( _pos < 0 || _pos >= _s.Length )
                _currentChar = null;
            else
                _currentChar = _s[_pos];
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        private bool SeekCompare( string target )
        {
            if( _pos + target.Length > _s.Length )
                return false;

            return _s[(_pos)..(_pos + target.Length)] == target;
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        private void EatWhiteSpace()
        {
            while( _currentChar != null && char.IsWhiteSpace( _currentChar.Value ) )
            {
                Advance();
            }
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public void EatJsonText()
        {
            EatWhiteSpace();

            EatValue();

            EatWhiteSpace();
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public void Eat_ArrayStart()
        {
            EatWhiteSpace();

            Contract.Assert( _currentChar == '[' );
            Advance();

            EatWhiteSpace();
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public void Eat_ArrayEnd()
        {
            EatWhiteSpace();

            Contract.Assert( _currentChar == ']' );
            Advance();

            EatWhiteSpace();
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public void Eat_ObjectStart()
        {
            EatWhiteSpace();

            Contract.Assert( _currentChar == '{' );
            Advance();

            EatWhiteSpace();
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public void Eat_ObjectEnd()
        {
            EatWhiteSpace();

            Contract.Assert( _currentChar == '}' );
            Advance();

            EatWhiteSpace();
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public void Eat_NameSeparator()
        {
            EatWhiteSpace();

            Contract.Assert( _currentChar == ':' );
            Advance();

            EatWhiteSpace();
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public void Eat_ValueSeparator()
        {
            EatWhiteSpace();

            Contract.Assert( _currentChar == ',' );
            Advance();

            EatWhiteSpace();
        }

        public SerializedValue EatValue()
        {
            if( SeekCompare( "false" ) )
            {
                Advance( "false".Length );
                return new SerializedValue( false );
            }
            if( SeekCompare( "true" ) )
            {
                Advance( "true".Length );
                return new SerializedValue( true );
            }
            if( SeekCompare( "null" ) )
            {
                Advance( "null".Length );
                return null;
            }
            if( _currentChar == '[' )
            {
                return new SerializedValue( EatArray() );
            }
            if( _currentChar == '{' )
            {
                return new SerializedValue( EatObject() );
            }
            if( _currentChar == '"' )
            {
                return new SerializedValue( EatString() );
            }
            if( _currentChar == '-' || (_currentChar != null && char.IsDigit( _currentChar.Value )) )
                return new SerializedValue( EatNumber() );

            throw new InvalidOperationException( $"Unexpected token at {_pos}." );
        }

        public SerializedObject EatObject()
        {
            Eat_ObjectStart();

            SerializedObject obj = new SerializedObject();

            while( true )
            {
                (string name, SerializedValue val) = EatMember();
                obj.Add( name, val );

                // value sep
                EatWhiteSpace();

                if( _currentChar == ',' )
                {
                    Advance();

                    EatWhiteSpace();
                    continue;
                }

                if( _currentChar == '}' )
                {
                    break;
                }
            }

            Eat_ObjectEnd();

            return obj;
        }

        public (string, SerializedValue) EatMember()
        {
            string name = EatString();

            Eat_NameSeparator();

            SerializedValue val = EatValue();

            return (name, val);
        }

        public SerializedArray EatArray()
        {
            Eat_ArrayStart();

            SerializedArray arr = new SerializedArray();

            while( true )
            {
                SerializedValue val = EatValue();
                arr.Add( val );

                // value sep
                EatWhiteSpace();

                if( _currentChar == ',' )
                {
                    Advance();

                    EatWhiteSpace();
                    continue;
                }

                if( _currentChar == ']' )
                {
                    break;
                }
            }

            Eat_ArrayEnd();

            return arr;
        }

        public string EatString()
        {
            Contract.Assert( _currentChar == '"' );
            Advance();

            int start = _pos;

            // Unescaped quote means the end of string
            while( _currentChar != '"' || (_previousChar == '\\') )
            {
                Advance();
            }

            string val = _s[start.._pos];
            // needs to un-escape other escaped sequences.

            Contract.Assert( _currentChar == '"' );
            Advance();

            return val;
        }

        public object EatNumber()
        {
            int start = _pos;
            bool hasDecimalPoint = false;
            bool hasExponent = false;

            if( _currentChar == '-' )
                Advance();

            EatInt();

            if( _currentChar == '.' )
            {
                hasDecimalPoint = true;
                Advance();

                if( _currentChar == null || !char.IsDigit( _currentChar.Value ) )
                {
                    throw new InvalidOperationException( $"Invalid token, a decimal point must be succeeded by a digit - {_pos}." );
                }

                EatInt();
            }

            if( _currentChar == 'e' || _currentChar == 'E' )
            {
                hasExponent = true;
                Advance();

                if( _currentChar != '+' && _currentChar != '-' )
                {
                    throw new InvalidOperationException( $"Invalid token, exponent 'e' must be succeeded by a plus/minus and a digit - {_pos}." );
                }

                Advance();

                if( _currentChar == null || !char.IsDigit( _currentChar.Value ) )
                {
                    throw new InvalidOperationException( $"Invalid token, exponent 'e' must be succeeded by a plus/minus and a digit - {_pos}." );
                }

                EatInt();

            }

            string val = _s[start..(_pos)];

            return (hasDecimalPoint || hasExponent) ? double.Parse( val, CultureInfo.InvariantCulture ) : long.Parse( val, CultureInfo.InvariantCulture );
        }

        private void EatInt()
        {
            while( char.IsDigit( _s[_pos] ) )
            {
                Advance();
            }
        }
    }
}
