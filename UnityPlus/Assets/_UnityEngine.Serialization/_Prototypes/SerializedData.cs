using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
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
        readonly object _value;

        public SerializedValue( object value )
        {
            this._value = value;
        }

        public object GetValue()
        {
            return _value;
        }

        public static implicit operator SerializedValue( bool v ) => new SerializedValue( v );
        public static implicit operator SerializedValue( sbyte v ) => new SerializedValue( v );
        public static implicit operator SerializedValue( byte v ) => new SerializedValue( v );
        public static implicit operator SerializedValue( short v ) => new SerializedValue( v );
        public static implicit operator SerializedValue( ushort v ) => new SerializedValue( v );
        public static implicit operator SerializedValue( int v ) => new SerializedValue( v );
        public static implicit operator SerializedValue( uint v ) => new SerializedValue( v );
        public static implicit operator SerializedValue( long v ) => new SerializedValue( v );
        public static implicit operator SerializedValue( ulong v ) => new SerializedValue( v );
        public static implicit operator SerializedValue( float v ) => new SerializedValue( v );
        public static implicit operator SerializedValue( double v ) => new SerializedValue( v );
        public static implicit operator SerializedValue( decimal v ) => new SerializedValue( v );
        public static implicit operator SerializedValue( string v ) => new SerializedValue( v );
        public static implicit operator SerializedValue( SerializedObject v ) => new SerializedValue( v );
        public static implicit operator SerializedValue( SerializedArray v ) => new SerializedValue( v );

        public static implicit operator bool( SerializedValue v ) => (bool)(v._value);
        public static implicit operator sbyte( SerializedValue v ) => (sbyte)((long)v._value);
        public static implicit operator byte( SerializedValue v ) => (byte)((long)v._value);
        public static implicit operator short( SerializedValue v ) => (short)((long)v._value);
        public static implicit operator ushort( SerializedValue v ) => (ushort)((long)v._value);
        public static implicit operator int( SerializedValue v ) => (int)((long)v._value);
        public static implicit operator uint( SerializedValue v ) => (uint)((long)v._value);
        public static implicit operator long( SerializedValue v ) => (long)(v._value);
        public static implicit operator ulong( SerializedValue v ) => (ulong)((long)v._value);
        public static implicit operator float( SerializedValue v ) => (float)((double)v._value);
        public static implicit operator double( SerializedValue v ) => (double)(v._value);
        public static implicit operator decimal( SerializedValue v ) => (decimal)((double)v._value);
        public static implicit operator string( SerializedValue v ) => (string)(v._value);
        public static implicit operator SerializedObject( SerializedValue v ) => (SerializedObject)(v._value);
        public static implicit operator SerializedArray( SerializedValue v ) => (SerializedArray)(v._value);

        public override bool Equals( object obj )
        {
            if( obj is SerializedValue val )
            {
                if( this._value == null )
                {
                    return val._value == null;
                }
                return this._value.Equals( val._value );
            }

            return false;
        }

        public override int GetHashCode()
        {
            return this._value.GetHashCode();
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
