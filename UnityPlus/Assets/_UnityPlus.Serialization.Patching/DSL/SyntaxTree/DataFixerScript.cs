using System.Collections.Generic;

namespace UnityPlus.Serialization.Patching.DSL.SyntaxTree
{
    /// <summary>
    /// Represents a DataFixer script file.
    /// </summary>
    public class DataFixerScript
    {
        public IStatement[] Statements { get; set; }

        public void InvokeOn( SerializedData rootPivot )
        {
            IEnumerable<TrackedSerializedData> pivot = new TrackedSerializedData[] { new TrackedSerializedData( rootPivot ) };

            foreach( var transformation in Statements )
            {
                transformation.Invoke( pivot );
            }
        }

        public void InvokeOn( TrackedSerializedData rootPivot )
        {
            IEnumerable<TrackedSerializedData> pivot = new TrackedSerializedData[] { rootPivot };

            foreach( var transformation in Statements )
            {
                transformation.Invoke( pivot );
            }
        }
    }
}