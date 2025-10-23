using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace UnityPlus.Serialization.Patching
{
    /// <summary>
    /// Represents a way to access one or multiple child elements of a serialized data object.
    /// </summary>
    /// <example>
    /// 
    ///     any
    ///     this
    ///     this.a
    ///     a[0]
    ///     a[0].b
    ///     "hello world"[*]
    ///     "hello world"[2..]."hi there"
    ///     "hello world"[2..5]."hi there"
    /// 
    /// </example>
    public readonly struct SerializedDataPath : IEquatable<SerializedDataPath>
    {
        private readonly SerializedDataPathSegment[] _segments;

        public IReadOnlyList<SerializedDataPathSegment> Segments => _segments;

        /// <summary>
        /// Constructs a path from the provided segments.
        /// </summary>
        public SerializedDataPath( params SerializedDataPathSegment[] segments )
        {
            _segments = segments ?? Array.Empty<SerializedDataPathSegment>();
        }

        /// <summary>
        /// Constructs a path from an enumerable of segments.
        /// </summary>
        public SerializedDataPath( IEnumerable<SerializedDataPathSegment> segments )
        {
            _segments = (segments ?? Enumerable.Empty<SerializedDataPathSegment>()).ToArray();
        }

        /// <summary>
        /// Evaluates a path on a single pivot.
        /// </summary>
        public IEnumerable<TrackedSerializedData> Evaluate( TrackedSerializedData pivotItem )
        {
            if( _segments == null || _segments.Length == 0 )
                return new TrackedSerializedData[] { pivotItem };

            IEnumerable<TrackedSerializedData> pivot = _segments[0].Evaluate( pivotItem );
            foreach( var segment in _segments.Skip( 1 ) )
            {
                pivot = segment.Evaluate( pivot );
            }
            return pivot;
        }

        /// <summary>
        /// Evaluates a path on multiple pivots, returns a flattened sequence of results from each pivot element in sequence.
        /// </summary>
        public IEnumerable<TrackedSerializedData> Evaluate( IEnumerable<TrackedSerializedData> pivot )
        {
            if( _segments == null || _segments.Length == 0 )
                return pivot;

            foreach( var segment in _segments )
            {
                pivot = segment.Evaluate( pivot );
            }
            return pivot;
        }

        public bool Equals( SerializedDataPath other )
        {
            if( ReferenceEquals( this, other ) )
                return true;

            if( _segments.Length != other._segments.Length )
                return false;

            for( int i = 0; i < _segments.Length; i++ )
            {
                if( !_segments[i].Equals( other._segments[i] ) )
                    return false;
            }

            return true;
        }

        public override bool Equals( object obj )
        {
            if( obj is SerializedDataPath other )
            {
                return Equals( other );
            }
            return false;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine( _segments );
        }

        public static SerializedDataPath Parse( string s )
        {
            if( s == null )
                throw new ArgumentNullException( nameof( s ) );

            int pos = 0;

            bool EatPrefix( out SerializedDataPathSegment segment )
            {
                segment = default;

                // corrected length checks (ensure enough characters remain)
                if( s.Length - pos >= 3 )
                {
                    if( s[pos] == 'a' && s[pos + 1] == 'n' && s[pos + 2] == 'y' &&
                        (s.Length - pos == 3 || !IsIdentifierChar( s[pos + 3] )) ) // avoid matching "anyone"
                    {
                        segment = SerializedDataPathSegment.Any();
                        pos += 3;
                        return true;
                    }
                }

                if( s.Length - pos >= 4 )
                {
                    if( s[pos] == 't' && s[pos + 1] == 'h' && s[pos + 2] == 'i' && s[pos + 3] == 's' &&
                        (s.Length - pos == 4 || !IsIdentifierChar( s[pos + 4] )) ) // avoid matching "thisis"
                    {
                        segment = SerializedDataPathSegment.This();
                        pos += 4;
                        return true;
                    }
                }

                return false;
            }

            bool EatDot()
            {
                if( pos + 1 < s.Length && s[pos] == '.' )
                {
                    pos++;
                    return true;
                }
                return false;
            }

            bool EatQuotedString( out SerializedDataPathSegment segment )
            {
                segment = default;
                if( pos >= s.Length || s[pos] != '"' )
                    return false;
                pos++; // eat opening quote

                var sb = new StringBuilder();
                while( pos < s.Length )
                {
                    char c = s[pos++];
                    if( c == '"' )
                    {
                        segment = SerializedDataPathSegment.Named( sb.ToString() );
                        return true;
                    }

                    if( c == '\\' )
                    {
                        if( pos >= s.Length ) throw new FormatException( "Invalid escape sequence at end of input." );
                        char esc = s[pos++];
                        switch( esc )
                        {
                            case '"': sb.Append( '"' ); break;
                            case '\\': sb.Append( '\\' ); break;
                            case '/': sb.Append( '/' ); break;
                            case 'b': sb.Append( '\b' ); break;
                            case 'f': sb.Append( '\f' ); break;
                            case 'n': sb.Append( '\n' ); break;
                            case 'r': sb.Append( '\r' ); break;
                            case 't': sb.Append( '\t' ); break;
                            case 'u':
                            {
                                // \uXXXX
                                if( pos + 4 > s.Length ) throw new FormatException( "Invalid unicode escape sequence." );
                                string hex = s.Substring( pos, 4 );
                                if( !int.TryParse( hex, System.Globalization.NumberStyles.HexNumber, null, out int code ) )
                                    throw new FormatException( "Invalid unicode escape sequence." );
                                sb.Append( (char)code );
                                pos += 4;
                                break;
                            }
                            default:
                                // Unknown escapes: allow verbatim of the escaped char
                                sb.Append( esc );
                                break;
                        }
                    }
                    else
                    {
                        sb.Append( c );
                    }
                }

                throw new FormatException( "Unterminated quoted string in path." );
            }

            bool EatBracketedIndexer( out SerializedDataPathSegment segment )
            {
                // [*] - any
                // [2] - index
                // [2..] - range from
                // [..5] - range to
                // [2..5] - range from-to
                // [2..5:5] - range from-to step

                segment = default;
                if( pos >= s.Length || s[pos] != '[' )
                    return false;
                pos++; // skip '['

                int startInner = pos;
                int bracketDepth = 1;
                // find closing ']' at same level (no nested allowed but we still search)
                while( pos < s.Length && bracketDepth > 0 )
                {
                    if( s[pos] == ']' )
                    {
                        bracketDepth--;
                        if( bracketDepth == 0 )
                            break;
                    }
                    else if( s[pos] == '[' )
                    {
                        // nested bracket is not expected, but handle by depth increment
                        bracketDepth++;
                    }
                    pos++;
                }

                if( pos >= s.Length || s[pos] != ']' )
                    throw new FormatException( "Unterminated bracketed indexer in path." );

                int endInner = pos; // position of closing ']' is pos
                string inner = s[startInner..endInner].Trim();
                pos++; // skip ']'

                if( inner.Length == 0 )
                    throw new FormatException( "Empty bracketed indexer." );

                if( inner == "*" )
                {
                    segment = SerializedDataPathSegment.IndexedAll();
                    return true;
                }


                // detect range by ".."
                int rangeSeparator = inner.IndexOf( ".." );
                if( rangeSeparator >= 0 )
                {
                    string rangeOnly = inner;

                    // optional step: indicated by ':' after range or index
                    int step = 1;
                    int colonIdx = inner.IndexOf( ':' );
                    if( colonIdx >= 0 )
                    {
                        rangeOnly = inner[..colonIdx].Trim();
                        string stepPart = inner[(colonIdx + 1)..].Trim();
                        if( stepPart.Length == 0 )
                            throw new FormatException( "Invalid step in indexer." );
                        if( !int.TryParse( stepPart, out step ) || step <= 0 )
                            throw new FormatException( $"Invalid step '{stepPart}' in indexer." );
                    }

                    string left = rangeOnly[..rangeSeparator].Trim();
                    string right = rangeOnly[(rangeSeparator + 2)..].Trim();

                    int indexMin;

                    if( left.Length == 0 && right.Length == 0 )
                        throw new FormatException( "Invalid range with no bounds in indexer." );

                    if( left.Length > 0 )
                    {
                        if( !int.TryParse( left, out indexMin ) )
                            throw new FormatException( $"Invalid range lower bound '{left}'." );
                    }
                    else
                    {
                        indexMin = 0;
                    }

                    int indexMax;

                    if( right.Length > 0 )
                    {
                        if( !int.TryParse( right, out indexMax ) )
                            throw new FormatException( $"Invalid range upper bound '{right}'." );
                    }
                    else
                    {
                        indexMax = int.MaxValue;
                    }

                    segment = SerializedDataPathSegment.IndexedRange( indexMin, indexMax, step );
                    return true;
                }

                // not a range -> single integer index (possibly with step? step ignored for single index)
                if( inner.StartsWith( '-' ) )
                    throw new FormatException( "Negative indices are not supported in indexer." );

                if( !int.TryParse( inner, out int idx ) )
                    throw new FormatException( $"Invalid index '{inner}' in indexer." );

                segment = SerializedDataPathSegment.Indexed( idx );
                return true;
            }

            bool EatNamedChild( out SerializedDataPathSegment segment )
            {
                segment = default;
                if( pos >= s.Length ) return false;

                int start = pos;
                // allow A-Za-z0-9_ characters (at least one)
                if( !IsIdentifierChar( s[pos] ) )
                    return false;

                pos++;
                while( pos < s.Length && IsIdentifierChar( s[pos] ) )
                    pos++;

                string name = s[start..pos];
                segment = SerializedDataPathSegment.Named( name );
                return true;
            }

            static bool IsIdentifierChar( char c )
            {
                return (c >= 'A' && c <= 'Z') ||
                       (c >= 'a' && c <= 'z') ||
                       (c >= '0' && c <= '9') ||
                       c == '_';
            }

            List<SerializedDataPathSegment> segments = new();

            if( EatPrefix( out var prefix ) )
                segments.Add( prefix );

            while( pos < s.Length )
            {
                if( pos >= s.Length )
                    break;

                if( s[pos] == '[' )
                {
                    if( !EatBracketedIndexer( out var idxSeg ) )
                        throw new FormatException( "Failed to parse bracketed indexer." );
                    segments.Add( idxSeg );
                    continue;
                }

                if( segments.Count > 0 && !EatDot() )
                    throw new FormatException( $"Expected '.' or '[' at position {pos} when parsing path." );

                if( s[pos] == '"' )
                {
                    if( !EatQuotedString( out var quoted ) )
                        throw new FormatException( "Failed to parse quoted string." );
                    segments.Add( quoted );
                    continue;
                }

                // otherwise named child
                if( EatNamedChild( out var named ) )
                {
                    segments.Add( named );
                    continue;
                }

                throw new FormatException( $"Unexpected character '{s[pos]}' at position {pos} when parsing path." );
            }

            return new SerializedDataPath( segments );
        }
    }
}