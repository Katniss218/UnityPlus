using System.Collections.Generic;

namespace UnityPlus.Serialization.Patching.DSL
{
    /// <summary>
    /// Represents an arbitrary type of path segment.
    /// </summary>
    public abstract class SerializedDataPathSegment
    {
        /// <summary>
        /// Evaluates a path on a single pivot.
        /// </summary>
        public abstract IEnumerable<TrackedSerializedData> Evaluate( TrackedSerializedData pivotItem );

        /// <summary>
        /// Evaluates a path on multiple pivots, returns a flattened sequence of results from each pivot element in sequence.
        /// </summary>
        public abstract IEnumerable<TrackedSerializedData> Evaluate( IEnumerable<TrackedSerializedData> pivot );
    }

    /// <summary>
    /// Represents the 'this' path segment. Returns the pivot item itself.
    /// </summary>
    public class ThisSerializedDataPathSegment : SerializedDataPathSegment
    {
        public ThisSerializedDataPathSegment()
        {
        }

        public override IEnumerable<TrackedSerializedData> Evaluate( TrackedSerializedData pivotItem )
        {
            yield return pivotItem;
        }

        public override IEnumerable<TrackedSerializedData> Evaluate( IEnumerable<TrackedSerializedData> pivot )
        {
            foreach( var pivotItem in pivot )
            {
                yield return pivotItem;
            }
        }
    }

    /// <summary>
    /// Represents the 'any' path segment. Returns all descendants (at any depth) of the pivot item.
    /// </summary>
    public class AnySerializationDataPathSegment : SerializedDataPathSegment
    {
        public override IEnumerable<TrackedSerializedData> Evaluate( TrackedSerializedData pivotItem )
        {
            return Traverse( pivotItem );
        }

        public override IEnumerable<TrackedSerializedData> Evaluate( IEnumerable<TrackedSerializedData> pivot )
        {
            foreach( var item in pivot )
            {
                foreach( var descendant in Traverse( item ) )
                {
                    yield return descendant;
                }
            }
        }

        private IEnumerable<TrackedSerializedData> Traverse( TrackedSerializedData root )
        {
            var stack = new Stack<TrackedSerializedData>();
            stack.Push( root );

            while( stack.Count > 0 )
            {
                var current = stack.Pop();

                // For each child of current
                for( int i = 0; i < current.value.Count; i++ )
                {
                    if( current.value.TryGetValue( i, out var childValue ) )
                    {
                        var child = new TrackedSerializedData( childValue, current.value, i );
                        yield return child;
                        stack.Push( child ); // Continue traversing this branch
                    }
                }
            }
        }
    }

    /// <summary>
    /// Represents a path segment that returns a child entry by its name.
    /// </summary>
    /// <remarks>
    /// If the pivot is not an object, or has no children, it returns nothing.
    /// </remarks>
    public class GlobalSerializedDataPathSegment : SerializedDataPathSegment
    {
        public GlobalSerializedDataPathSegment()
        {
        }

        public override IEnumerable<TrackedSerializedData> Evaluate( TrackedSerializedData pivotItem )
        {
            yield return new TrackedSerializedData( null ); // return the root element that the script was invoked with.
        }

        public override IEnumerable<TrackedSerializedData> Evaluate( IEnumerable<TrackedSerializedData> pivot )
        {
            foreach( var _ in pivot )
            {
                yield return new TrackedSerializedData( null ); // return the root element that the script was invoked with.
            }
        }
    }

    /// <summary>
    /// Represents a path segment that returns a child entry by its name.
    /// </summary>
    /// <remarks>
    /// If the pivot is not an object, or has no children, it returns nothing.
    /// </remarks>
    public class NamedSerializedDataPathSegment : SerializedDataPathSegment
    {
        public string Name { get; set; }

        public NamedSerializedDataPathSegment( string name )
        {
            this.Name = name;
        }

        public override IEnumerable<TrackedSerializedData> Evaluate( TrackedSerializedData pivotItem )
        {
            if( pivotItem.value.TryGetValue( Name, out var value ) )
                yield return new TrackedSerializedData( value, pivotItem.value, Name );
        }

        public override IEnumerable<TrackedSerializedData> Evaluate( IEnumerable<TrackedSerializedData> pivot )
        {
            foreach( var pivotItem in pivot )
            {
                if( pivotItem.value.TryGetValue( Name, out var value ) )
                    yield return new TrackedSerializedData( value, pivotItem.value, Name );
            }
        }
    }

    /// <summary>
    /// Represents a path segment that returns child entries by their index.
    /// </summary>
    /// <remarks>
    /// If the pivot is not an array, or has no children, it returns nothing.
    /// </remarks>
    public class IndexedSerializedDataPathSegment : SerializedDataPathSegment
    {
        /// <summary>
        /// Whether or not to return all child elements in the pivot - [*].
        /// </summary>
        public bool Every { get; set; }
        public int IndexMin { get; set; }
        public int IndexMax { get; set; }
        public int Step { get; set; } = 1;

        public static IndexedSerializedDataPathSegment All() => new IndexedSerializedDataPathSegment() { Every = true };

        protected IndexedSerializedDataPathSegment()
        {
        }

        public IndexedSerializedDataPathSegment( int index )
        {
            this.IndexMin = index;
            this.IndexMax = index;
        }

        public IndexedSerializedDataPathSegment( int indexMin, int indexMax )
        {
            this.IndexMin = indexMin;
            this.IndexMax = indexMax;
        }

        public IndexedSerializedDataPathSegment( int indexMin, int indexMax, int step )
        {
            this.IndexMin = indexMin;
            this.IndexMax = indexMax;
            this.Step = step;
        }

        public override IEnumerable<TrackedSerializedData> Evaluate( TrackedSerializedData pivotItem )
        {
            if( Every )
            {
                for( int i = 0; i < pivotItem.value.Count; i++ )
                {
                    if( pivotItem.value.TryGetValue( i, out var value ) )
                        yield return new TrackedSerializedData( value, pivotItem.value, i );
                }
            }
            if( IndexMin == IndexMax )
            {
                if( pivotItem.value.TryGetValue( IndexMin, out var value ) )
                    yield return new TrackedSerializedData( value, pivotItem.value, IndexMin );
            }
            else
            {
                for( int i = IndexMin; i < IndexMax; i += Step )
                {
                    if( pivotItem.value.TryGetValue( i, out var value ) )
                        yield return new TrackedSerializedData( value, pivotItem.value, i );
                }
            }
        }

        public override IEnumerable<TrackedSerializedData> Evaluate( IEnumerable<TrackedSerializedData> pivot )
        {
            foreach( var pivotItem in pivot )
            {
                if( Every )
                {
                    for( int i = 0; i < pivotItem.value.Count; i++ )
                    {
                        if( pivotItem.value.TryGetValue( i, out var value ) )
                            yield return new TrackedSerializedData( value, pivotItem.value, i );
                    }
                }
                if( IndexMin == IndexMax )
                {
                    if( pivotItem.value.TryGetValue( IndexMin, out var value ) )
                        yield return new TrackedSerializedData( value, pivotItem.value, IndexMin );
                }
                else
                {
                    for( int i = IndexMin; i < IndexMax; i += Step )
                    {
                        if( pivotItem.value.TryGetValue( i, out var value ) )
                            yield return new TrackedSerializedData( value, pivotItem.value, i );
                    }
                }
            }
        }
    }
}