using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace UnityEngine.Serialization
{
    public abstract class SerializedData
    {
        //public SerializedValue this[int index] { get => null; set => _ = value; }
        //public SerializedValue this[string name] { get => null; set => _ = value; }
    }

    public class SerializedValue : SerializedData
    // any value:
    // - boolean `true`
    // - number `123.456`
    // - string `"string"`
    // - object `{ serializedObject }`
    // - array `[ serializedArray ]`
    {
        [StructLayout( LayoutKind.Explicit )]
        struct Union
        {
            [FieldOffset( 0 )] public Guid equalityChecker;

            [FieldOffset( 0 )] public bool boolean;
            [FieldOffset( 0 )] public long @int;
            [FieldOffset( 0 )] public ulong @uint;
            [FieldOffset( 0 )] public double @float;
            [FieldOffset( 0 )] public decimal @decimal;
            [FieldOffset( 16 )] public object obj; // decimal is a big boi, 16 bytes offset.
        }

        enum DataType : byte
        {
            Invalid = 0,

            Boolean,

            Int,
            UInt,

            Float,
            Decimal,

            String = 128,

            Object,
            Array
        }

        readonly Union _value;
        readonly DataType _valueType;

        SerializedValue( Union value, DataType valueType )
        {
            this._value = value;
            this._valueType = valueType;
        }

        public static implicit operator SerializedValue( bool v ) => new SerializedValue( new Union() { boolean = v }, DataType.Boolean );
        public static implicit operator SerializedValue( sbyte v ) => new SerializedValue( new Union() { @int = v }, DataType.Int );
        public static implicit operator SerializedValue( byte v ) => new SerializedValue( new Union() { @uint = v }, DataType.UInt );
        public static implicit operator SerializedValue( short v ) => new SerializedValue( new Union() { @int = v }, DataType.Int );
        public static implicit operator SerializedValue( ushort v ) => new SerializedValue( new Union() { @uint = v }, DataType.UInt );
        public static implicit operator SerializedValue( int v ) => new SerializedValue( new Union() { @int = v }, DataType.Int );
        public static implicit operator SerializedValue( uint v ) => new SerializedValue( new Union() { @uint = v }, DataType.UInt );
        public static implicit operator SerializedValue( long v ) => new SerializedValue( new Union() { @int = v }, DataType.Int );
        public static implicit operator SerializedValue( ulong v ) => new SerializedValue( new Union() { @uint = v }, DataType.UInt );
        public static implicit operator SerializedValue( float v ) => new SerializedValue( new Union() { @float = v }, DataType.Float );
        public static implicit operator SerializedValue( double v ) => new SerializedValue( new Union() { @float = v }, DataType.Float );
        public static implicit operator SerializedValue( decimal v ) => new SerializedValue( new Union() { @decimal = v }, DataType.Decimal );
        public static implicit operator SerializedValue( string v ) => new SerializedValue( new Union() { obj = v }, DataType.String );
        public static implicit operator SerializedValue( SerializedObject v ) => new SerializedValue( new Union() { obj = v }, DataType.Object );
        public static implicit operator SerializedValue( SerializedArray v ) => new SerializedValue( new Union() { obj = v }, DataType.Array );

        public static implicit operator bool( SerializedValue v ) => v._valueType switch
        {
            DataType.Boolean => v._value.boolean,
            _ => throw new InvalidOperationException( $"Can't convert to `bool` from `{v._valueType}`." ),
        };
        public static implicit operator sbyte( SerializedValue v ) => v._valueType switch
        {
            DataType.Int => (sbyte)v._value.@int,
            DataType.UInt => (sbyte)v._value.@uint,
            DataType.Float => (sbyte)v._value.@float,
            DataType.Decimal => (sbyte)v._value.@decimal,
            _ => throw new InvalidOperationException( $"Can't convert to `sbyte` from `{v._valueType}`." ),
        };
        public static implicit operator byte( SerializedValue v ) => v._valueType switch
        {
            DataType.Int => (byte)v._value.@int,
            DataType.UInt => (byte)v._value.@uint,
            DataType.Float => (byte)v._value.@float,
            DataType.Decimal => (byte)v._value.@decimal,
            _ => throw new InvalidOperationException( $"Can't convert to `byte` from `{v._valueType}`." ),
        };
        public static implicit operator short( SerializedValue v ) => v._valueType switch
        {
            DataType.Int => (short)v._value.@int,
            DataType.UInt => (short)v._value.@uint,
            DataType.Float => (short)v._value.@float,
            DataType.Decimal => (short)v._value.@decimal,
            _ => throw new InvalidOperationException( $"Can't convert to `short` from `{v._valueType}`." ),
        };
        public static implicit operator ushort( SerializedValue v ) => v._valueType switch
        {
            DataType.Int => (ushort)v._value.@int,
            DataType.UInt => (ushort)v._value.@uint,
            DataType.Float => (ushort)v._value.@float,
            DataType.Decimal => (ushort)v._value.@decimal,
            _ => throw new InvalidOperationException( $"Can't convert to `ushort` from `{v._valueType}`." ),
        };
        public static implicit operator int( SerializedValue v ) => v._valueType switch
        {
            DataType.Int => (int)v._value.@int,
            DataType.UInt => (int)v._value.@uint,
            DataType.Float => (int)v._value.@float,
            DataType.Decimal => (int)v._value.@decimal,
            _ => throw new InvalidOperationException( $"Can't convert to `int` from `{v._valueType}`." ),
        };
        public static implicit operator uint( SerializedValue v ) => v._valueType switch
        {
            DataType.Int => (uint)v._value.@int,
            DataType.UInt => (uint)v._value.@uint,
            DataType.Float => (uint)v._value.@float,
            DataType.Decimal => (uint)v._value.@decimal,
            _ => throw new InvalidOperationException( $"Can't convert to `uint` from `{v._valueType}`." ),
        };
        public static implicit operator long( SerializedValue v ) => v._valueType switch
        {
            DataType.Int => (long)v._value.@int,
            DataType.UInt => (long)v._value.@uint,
            DataType.Float => (long)v._value.@float,
            DataType.Decimal => (long)v._value.@decimal,
            _ => throw new InvalidOperationException( $"Can't convert to `long` from `{v._valueType}`." ),
        };
        public static implicit operator ulong( SerializedValue v ) => v._valueType switch
        {
            DataType.Int => (ulong)v._value.@int,
            DataType.UInt => (ulong)v._value.@uint,
            DataType.Float => (ulong)v._value.@float,
            DataType.Decimal => (ulong)v._value.@decimal,
            _ => throw new InvalidOperationException( $"Can't convert to `ulong` from `{v._valueType}`." ),
        };
        public static implicit operator float( SerializedValue v ) => v._valueType switch
        {
            DataType.Int => (float)v._value.@int,
            DataType.UInt => (float)v._value.@uint,
            DataType.Float => (float)v._value.@float,
            DataType.Decimal => (float)v._value.@decimal,
            _ => throw new InvalidOperationException( $"Can't convert to `float` from `{v._valueType}`." ),
        };
        public static implicit operator double( SerializedValue v ) => v._valueType switch
        {
            DataType.Int => (double)v._value.@int,
            DataType.UInt => (double)v._value.@uint,
            DataType.Float => (double)v._value.@float,
            DataType.Decimal => (double)v._value.@decimal,
            _ => throw new InvalidOperationException( $"Can't convert to `double` from `{v._valueType}`." ),
        };
        public static implicit operator decimal( SerializedValue v ) => v._valueType switch
        {
            DataType.Int => (decimal)v._value.@int,
            DataType.UInt => (decimal)v._value.@uint,
            DataType.Float => (decimal)v._value.@float,
            DataType.Decimal => (decimal)v._value.@decimal,
            _ => throw new InvalidOperationException( $"Can't convert to `decimal` from `{v._valueType}`." ),
        };
        public static implicit operator string( SerializedValue v ) => v._valueType switch
        {
            DataType.String => (string)v._value.obj,
            _ => throw new InvalidOperationException( $"Can't convert to `string` from `{v._valueType}`." ),
        };
        public static implicit operator SerializedObject( SerializedValue v ) => v._valueType switch
        {
            DataType.Object => (SerializedObject)v._value.obj,
            _ => throw new InvalidOperationException( $"Can't convert to `{nameof( SerializedObject )}` from `{v._valueType}`." ),
        };
        public static implicit operator SerializedArray( SerializedValue v ) => v._valueType switch
        {
            DataType.Array => (SerializedArray)v._value.obj,
            _ => throw new InvalidOperationException( $"Can't convert to `{nameof( SerializedArray )}` from `{v._valueType}`." ),
        };

        public override bool Equals( object obj )
        {
            if( obj is SerializedValue val )
            {
                return this == val;
            }

            return false;
        }

        public override int GetHashCode()
        {
            return this._value.obj.GetHashCode();
        }

        public static bool operator ==( SerializedValue v1, SerializedValue v2 )
        {
            if( v1._valueType >= DataType.String ) // reference types > 128
            {
                if( v1._value.obj == null )
                    return v2._value.obj == null;
                return v1._value.obj.Equals( v2._value.obj );
            }
            return v1._value.equalityChecker == v2._value.equalityChecker;
        }

        public static bool operator !=( SerializedValue v1, SerializedValue v2 )
        {
            return !(v1 == v2);
        }
    }

    public class SerializedObject : SerializedData, IDictionary<string, SerializedValue>
    {
        readonly Dictionary<string, SerializedValue> _children;

        public ICollection<string> Keys => _children.Keys;
        public ICollection<SerializedValue> Values => _children.Values;
        public int Count => _children.Count;
        public bool IsReadOnly => ((ICollection<KeyValuePair<string, SerializedValue>>)_children).IsReadOnly;

        public SerializedValue this[string key] { get { return _children[key]; } set { _children[key] = value; } }

        public SerializedObject()
        {
            _children = new Dictionary<string, SerializedValue>();
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public void Add( string key, SerializedValue value )
        {
            _children.Add( key, value );
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public void Add( KeyValuePair<string, SerializedValue> item )
        {
            ((ICollection<KeyValuePair<string, SerializedValue>>)_children).Add( item );
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public void Clear()
        {
            _children.Clear();
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public bool Contains( KeyValuePair<string, SerializedValue> item )
        {
            return _children.Contains( item );
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public bool ContainsKey( string key )
        {
            return _children.ContainsKey( key );
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public void CopyTo( KeyValuePair<string, SerializedValue>[] array, int arrayIndex )
        {
            ((ICollection<KeyValuePair<string, SerializedValue>>)_children).CopyTo( array, arrayIndex );
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public IEnumerator<KeyValuePair<string, SerializedValue>> GetEnumerator()
        {
            return _children.GetEnumerator();
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public bool Remove( string key )
        {
            return _children.Remove( key );
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public bool Remove( KeyValuePair<string, SerializedValue> item )
        {
            return ((ICollection<KeyValuePair<string, SerializedValue>>)_children).Remove( item );
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public bool TryGetValue( string key, out SerializedValue value )
        {
            return _children.TryGetValue( key, out value );
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)_children).GetEnumerator();
        }
    }

    public class SerializedArray : SerializedData, IList<SerializedValue>
    {
        readonly List<SerializedValue> _children;

        public int Count => _children.Count;
        public bool IsReadOnly => ((ICollection<SerializedValue>)_children).IsReadOnly;

        public SerializedArray()
        {
            _children = new List<SerializedValue>();
        }

        public SerializedValue this[int index] { get { return _children[index]; } set { _children[index] = value; } }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public void Add( SerializedValue item )
        {
            _children.Add( item );
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public void Clear()
        {
            _children.Clear();
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public bool Contains( SerializedValue item )
        {
            return _children.Contains( item );
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public void CopyTo( SerializedValue[] array, int arrayIndex )
        {
            _children.CopyTo( array, arrayIndex );
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public IEnumerator<SerializedValue> GetEnumerator()
        {
            return _children.GetEnumerator();
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public int IndexOf( SerializedValue item )
        {
            return _children.IndexOf( item );
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public void Insert( int index, SerializedValue item )
        {
            _children.Insert( index, item );
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public bool Remove( SerializedValue item )
        {
            return _children.Remove( item );
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public void RemoveAt( int index )
        {
            _children.RemoveAt( index );
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)_children).GetEnumerator();
        }
    }
}
