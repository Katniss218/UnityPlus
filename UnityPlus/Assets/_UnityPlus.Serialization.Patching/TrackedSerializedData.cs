using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace UnityPlus.Serialization.Patching
{
    /// <summary>
    /// A struct that keeps track of the parent of the current item, to allow the current item to be changed in the hierarchy.
    /// </summary>
    public readonly struct TrackedSerializedData : IEquatable<TrackedSerializedData>
    {
        // This entire struct is kind of a hack
        // - Needs to support `this = blah;` but the 'parent' can't be stored in the SerializedData node.

        /// <summary>
        /// The main value that this struct is tracking.
        /// </summary>
        public readonly SerializedData value;

        /// <summary>
        /// The parent of this value in the serialized data hierarchy.
        /// </summary>
        public readonly SerializedData parent;

        /// <summary>
        /// The root of the serialized data hierarchy that this serialized data tracks.
        /// </summary>
        public readonly SerializedData root;

        /// <summary>
        /// The name under which this value exists in its parent (if applicable).
        /// </summary>
        public readonly string name;

        /// <summary>
        /// The index under which this value exists in its parent (if applicable).
        /// </summary>
        public readonly int index;

        /// <summary>
        /// Whether this value exists in its parent by index (true) or by name (false).
        /// </summary>
        public readonly bool IsByIndex => index != -1;

        /// <summary>
        /// Whether this value exists in its parent by name (true) or by index (false).
        /// </summary>
        public readonly bool IsByName => index == -1;

        /// <summary>
        /// Creates a tracked serialized data from a root value that doesn't have a parent.
        /// </summary>
        public TrackedSerializedData( SerializedData rootValue )
        {
            this.value = rootValue;
            this.parent = null;
            this.root = rootValue;
            this.name = null;
            this.index = -1;
        }

        /// <summary>
        /// Creates a tracked serialized data from a value, its parent and the name under which it exist in the parent.
        /// </summary>
        private TrackedSerializedData( SerializedData value, SerializedData parent, string name, SerializedData root )
        {
            this.value = value;
            this.parent = parent;
            this.root = root;
            this.name = name;
            this.index = -1;
        }

        /// <summary>
        /// Creates a tracked serialized data from a value, its parent and the index under which it exist in the parent.
        /// </summary>
        private TrackedSerializedData( SerializedData value, SerializedData parent, int index, SerializedData root )
        {
            this.value = value;
            this.parent = parent;
            this.root = root;
            this.name = null;
            this.index = index;
        }


        /// <summary>
        /// Accesses a child element by its index (if applicable).
        /// </summary>
        public TrackedSerializedData this[int index]
        {
            [MethodImpl( MethodImplOptions.AggressiveInlining )]
            get => new TrackedSerializedData( value[index], value, index, root );
            [MethodImpl( MethodImplOptions.AggressiveInlining )]
            set => value[index] = value;
        }

        /// <summary>
        /// Accesses a child element by its name (if applicable).
        /// </summary>
        public TrackedSerializedData this[string name]
        {
            [MethodImpl( MethodImplOptions.AggressiveInlining )]
            get => new TrackedSerializedData( value[name], value, name, root );
            [MethodImpl( MethodImplOptions.AggressiveInlining )]
            set => value[name] = value;
        }

        /// <summary>
        /// Tries to access a child element by its name.
        /// </summary>
        /// <param name="name">The name of the child element to get.</param>
        /// <param name="value">The child element (if the returned value was true).</param>
        /// <returns>True if the specified child element exists.</returns>
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public bool TryGetValue( string name, out TrackedSerializedData value )
        {
            if( this.value.TryGetValue( name, out var childValue ) )
            {
                value = new TrackedSerializedData( childValue, this.value, name, root );
                return true;
            }
            value = default;
            return false;
        }

        /// <summary>
        /// Tries to access a child element by its index.
        /// </summary>
        /// <param name="index">The index of the child element to get.</param>
        /// <param name="value">The child element (if the returned value was true).</param>
        /// <returns>True if the specified child element exists.</returns>
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public bool TryGetValue( int index, out TrackedSerializedData value )
        {
            if( this.value.TryGetValue( index, out var childValue ) )
            {
                value = new TrackedSerializedData( childValue, this.value, index, root );
                return true;
            }
            value = default;
            return false;
        }

        /// <summary>
        /// Enumerates all child elements of this value (if applicable).
        /// </summary>
        public IEnumerable<TrackedSerializedData> EnumerateChildren()
        {
            if( value is SerializedPrimitive )
                yield break;

            if( value is SerializedObject obj )
            {
                foreach( var kvp in obj )
                {
                    yield return new TrackedSerializedData( kvp.Value, value, kvp.Key, root );
                }
                yield break;
            }
            if( value is SerializedArray arr )
            {
                for( int i = 0; i < arr.Count; ++i )
                {
                    yield return new TrackedSerializedData( arr[i], value, i, root );
                }
                yield break;
            }
        }

        public bool Equals( TrackedSerializedData other )
        {
            if( value == null )
                return other.value == null;
            if( other.value == null )
                return false;

            return value.Equals( other.value );
        }

        public override bool Equals( object obj )
        {
            if( obj is TrackedSerializedData other )
            {
                return Equals( other );
            }
            return false;
        }

        public override int GetHashCode()
        {
            return value != null ? value.GetHashCode() : 0;
        }

        public static bool operator ==( TrackedSerializedData left, TrackedSerializedData right )
        {
            return left.Equals( right );
        }

        public static bool operator !=( TrackedSerializedData left, TrackedSerializedData right )
        {
            return !( left == right );
        }
    }
}