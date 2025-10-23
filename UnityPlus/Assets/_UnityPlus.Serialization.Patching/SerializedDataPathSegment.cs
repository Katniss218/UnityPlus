using System;
using System.Collections.Generic;

namespace UnityPlus.Serialization.Patching
{
    /// <summary>
    /// A single struct representation of all possible serialized-data path segments.
    /// Replaces the inheritance-based segment classes with a discriminated union style struct.
    /// </summary>
    public readonly struct SerializedDataPathSegment : IEquatable<SerializedDataPathSegment>
    {
        public enum KindEnum : byte
        {
            This,
            Any,
            Global,
            Named,
            Indexed
        }

        public KindEnum Kind { get; }

        // Named segment
        public string Name { get; }

        // Indexed segment fields
        public bool Every { get; }
        public int IndexMin { get; }
        public int IndexMax { get; }
        public int Step { get; }

        private SerializedDataPathSegment( KindEnum kind, string name = null, bool every = false, int indexMin = 0, int indexMax = 0, int step = 1 )
        {
            Kind = kind;
            Name = name;
            Every = every;
            IndexMin = indexMin;
            IndexMax = indexMax;
            Step = step;
        }

        public static SerializedDataPathSegment This() => new SerializedDataPathSegment( KindEnum.This );

        public static SerializedDataPathSegment Any() => new SerializedDataPathSegment( KindEnum.Any );

        /// <summary>
        /// TODO: this should return the root element the path was evaluated against.
        /// Current placeholder matches previous behavior in the inheritance code.
        /// </summary>
        public static SerializedDataPathSegment Global() => new SerializedDataPathSegment( KindEnum.Global );

        public static SerializedDataPathSegment Named( string name )
        {
            if( name == null )
                throw new ArgumentNullException( nameof( name ) );
            return new SerializedDataPathSegment( KindEnum.Named, name: name );
        }

        /// <summary>
        /// Single index.
        /// </summary>
        public static SerializedDataPathSegment Indexed( int index )
            => new SerializedDataPathSegment( KindEnum.Indexed, every: false, indexMin: index, indexMax: index, step: 1 );

        /// <summary>
        /// Range [indexMin, indexMax) with step (indexMax is treated as exclusive consistent with original code).
        /// </summary>
        public static SerializedDataPathSegment IndexedRange( int indexMin, int indexMax, int step = 1 )
            => new SerializedDataPathSegment( KindEnum.Indexed, every: false, indexMin: indexMin, indexMax: indexMax, step: step );

        /// <summary>
        /// All elements (the [*] operator).
        /// </summary>
        public static SerializedDataPathSegment IndexedAll()
            => new SerializedDataPathSegment( KindEnum.Indexed, every: true, indexMin: 0, indexMax: 0, step: 1 );

        /// <summary>
        /// Evaluate this segment for a single pivot item.
        /// </summary>
        public IEnumerable<TrackedSerializedData> Evaluate( TrackedSerializedData pivotItem )
        {
            switch( Kind )
            {
                case KindEnum.This:
                    yield return pivotItem;
                    yield break;

                case KindEnum.Any:
                    foreach( var d in TraverseOne( pivotItem ) )
                        yield return d;
                    yield break;

                case KindEnum.Global:
#warning TODO - return the root element that the script was invoked with.
                    yield return new TrackedSerializedData( null );
                    yield break;

                case KindEnum.Named:
                    if( pivotItem.TryGetValue( Name, out var namedValue ) )
                        yield return namedValue;
                    yield break;

                case KindEnum.Indexed:
                    if( Every )
                    {
                        for( int i = 0; i < pivotItem.value.Count; i++ )
                        {
                            if( pivotItem.TryGetValue( i, out var v ) )
                                yield return v;
                        }
                        yield break;
                    }

                    if( IndexMin == IndexMax )
                    {
                        if( pivotItem.TryGetValue( IndexMin, out var single ) )
                            yield return single;
                        yield break;
                    }
                    else
                    {
                        for( int i = IndexMin; i < IndexMax && i < pivotItem.value.Count; i += Math.Max( 1, Step ) )
                        {
                            if( pivotItem.TryGetValue( i, out var v ) )
                                yield return v;
                        }
                        yield break;
                    }

                default:
                    yield break;
            }
        }

        /// <summary>
        /// Evaluate this segment for multiple pivots (flattened).
        /// </summary>
        public IEnumerable<TrackedSerializedData> Evaluate( IEnumerable<TrackedSerializedData> pivot )
        {
            if( pivot == null ) yield break;

            switch( Kind )
            {
                case KindEnum.This:
                    foreach( var p in pivot ) yield return p;
                    yield break;

                case KindEnum.Any:
                    foreach( var p in pivot )
                    {
                        foreach( var d in TraverseOne( p ) )
                            yield return d;
                    }
                    yield break;

                case KindEnum.Global:
#warning TODO - return the root element that the script was invoked with.
                    foreach( var _ in pivot )
                        yield return new TrackedSerializedData( null );
                    yield break;

                case KindEnum.Named:
                    foreach( var p in pivot )
                    {
                        if( p.TryGetValue( Name, out var value ) )
                            yield return value;
                    }
                    yield break;

                case KindEnum.Indexed:
                    foreach( var p in pivot )
                    {
                        if( Every )
                        {
                            for( int i = 0; i < p.value.Count; i++ )
                            {
                                if( p.TryGetValue( i, out var v ) )
                                    yield return v;
                            }
                            continue;
                        }

                        if( IndexMin == IndexMax )
                        {
                            if( p.TryGetValue( IndexMin, out var single ) )
                                yield return single;
                            continue;
                        }
                        else
                        {
                            for( int i = IndexMin; i < IndexMax && i < p.value.Count; i += Math.Max( 1, Step ) )
                            {
                                if( p.TryGetValue( i, out var v ) )
                                    yield return v;
                            }
                        }
                    }
                    yield break;

                default:
                    yield break;
            }
        }


        private IEnumerable<TrackedSerializedData> TraverseOne( TrackedSerializedData root )
        {
            // Depth-first traversal that returns descendants including immediate children.
            Stack<TrackedSerializedData> stack = new();
            stack.Push( root );

            while( stack.Count > 0 )
            {
                TrackedSerializedData current = stack.Pop();

                foreach( var child in current.EnumerateChildren() )
                {
                    yield return child;
                    stack.Push( child ); // Continue traversing this branch
                }
            }
        }

        public bool Equals( SerializedDataPathSegment other )
        {
            if( Kind != other.Kind )
                return false;

            switch( Kind )
            {
                case KindEnum.Named:
                    return string.Equals( Name, other.Name, StringComparison.Ordinal );
                case KindEnum.Indexed:
                    return Every == other.Every
                        && IndexMin == other.IndexMin
                        && IndexMax == other.IndexMax
                        && Step == other.Step;
                default:
                    return true;
            }
        }

        public override bool Equals( object obj )
        {
            if( obj is SerializedDataPathSegment other )
            {
                return Equals( other );
            }
            return false;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = (int)Kind;
                if( Kind == KindEnum.Named && Name != null )
                    hash = (hash * 397) ^ Name.GetHashCode();
                if( Kind == KindEnum.Indexed )
                {
                    hash = (hash * 397) ^ Every.GetHashCode();
                    hash = (hash * 397) ^ IndexMin;
                    hash = (hash * 397) ^ IndexMax;
                    hash = (hash * 397) ^ Step;
                }
                return hash;
            }
        }

        public override string ToString()
        {
            switch( Kind )
            {
                case KindEnum.This: return "this";
                case KindEnum.Any: return "any";
                case KindEnum.Global: return "global";
                case KindEnum.Named: return Name ?? string.Empty;
                case KindEnum.Indexed:
                    if( Every ) return "[*]";
                    if( IndexMin == IndexMax ) return $"[{IndexMin}]";
                    return $"[{IndexMin}..{IndexMax}{(Step != 1 ? $":{Step}" : "")}]";
                default: return string.Empty;
            }
        }

        public static bool operator ==( SerializedDataPathSegment a, SerializedDataPathSegment b ) => a.Equals( b );
        public static bool operator !=( SerializedDataPathSegment a, SerializedDataPathSegment b ) => !a.Equals( b );
    }
}