using System.Collections.Generic;

namespace UnityPlus.Serialization.Patching.DSL
{
    public abstract class SerializedDataPathSegment
    {
        public abstract IEnumerable<TrackedSerializedData> Evaluate( TrackedSerializedData pivotItem );
        public abstract IEnumerable<TrackedSerializedData> Evaluate( IEnumerable<TrackedSerializedData> pivot );
    }

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

    public class AnySerializationDataPathSegment : SerializedDataPathSegment
    {
#warning TODO - this should flatten *everything*
        public override IEnumerable<TrackedSerializedData> Evaluate( TrackedSerializedData pivotItem )
        {
            for( int i = 0; i < pivotItem.value.Count; i++ )
            {
                if( pivotItem.value.TryGetValue( i, out var value ) )
                    yield return new TrackedSerializedData( value, pivotItem.value, i );
            }
        }

        public override IEnumerable<TrackedSerializedData> Evaluate( IEnumerable<TrackedSerializedData> pivot )
        {
            foreach( var pivotItem in pivot )
            {
                for( int i = 0; i < pivotItem.value.Count; i++ )
                {
                    if( pivotItem.value.TryGetValue( i, out var value ) )
                        yield return new TrackedSerializedData( value, pivotItem.value, i );
                }
            }
        }
    }

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

    public class IndexedSerializedDataPathSegment : SerializedDataPathSegment
    {
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