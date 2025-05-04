using System.Collections.Generic;
using System.Linq;

namespace UnityPlus.Serialization.Patching.DSL
{
    public class SerializedDataPath
    {
        public SerializedDataPathSegment[] Segments { get; set; }

        public IEnumerable<TrackedSerializedData> Evaluate( TrackedSerializedData pivotItem )
        {
            if( Segments == null || Segments.Length == 0 )
                return new TrackedSerializedData[] { pivotItem };

            IEnumerable<TrackedSerializedData> pivot = Segments[0].Evaluate( pivotItem );
            foreach( var segment in Segments.Skip( 1 ) )
            {
                pivot = segment.Evaluate( pivot );
            }
            return pivot;
        }

        public IEnumerable<TrackedSerializedData> Evaluate( IEnumerable<TrackedSerializedData> pivot )
        {
            if( Segments == null || Segments.Length == 0 )
                return pivot;

            foreach( var segment in Segments )
            {
                pivot = segment.Evaluate( pivot );
            }
            return pivot;
        }
    }
}