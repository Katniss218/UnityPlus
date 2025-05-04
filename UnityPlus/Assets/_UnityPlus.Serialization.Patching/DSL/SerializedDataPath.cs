using System.Collections.Generic;
using System.Linq;

namespace UnityPlus.Serialization.Patching.DSL
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
    public class SerializedDataPath
    {
        public SerializedDataPathSegment[] Segments { get; set; }

        /// <summary>
        /// Evaluates a path on a single pivot.
        /// </summary>
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

        /// <summary>
        /// Evaluates a path on multiple pivots, returns a flattened sequence of results from each pivot element in sequence.
        /// </summary>
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