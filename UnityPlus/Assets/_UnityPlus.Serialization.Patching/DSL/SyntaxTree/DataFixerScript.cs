using System.Collections.Generic;

namespace UnityPlus.Serialization.Patching.DSL.SyntaxTree
{
    public class DataFixerScript
    {
        public Transformation[] Transformations { get; set; }

        public void InvokeOn( SerializedData rootPivot )
        {
            IEnumerable<TrackedSerializedData> pivot = new TrackedSerializedData[] { new TrackedSerializedData( rootPivot ) };

            foreach( var transformation in Transformations )
            {
                transformation.Invoke( pivot );
            }
        }

        public void InvokeOn( TrackedSerializedData rootPivot )
        {
            IEnumerable<TrackedSerializedData> pivot = new TrackedSerializedData[] { rootPivot };

            foreach( var transformation in Transformations )
            {
                transformation.Invoke( pivot );
            }
        }
    }
}