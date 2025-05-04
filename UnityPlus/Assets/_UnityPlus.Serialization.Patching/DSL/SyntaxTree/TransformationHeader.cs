using System.Collections.Generic;

namespace UnityPlus.Serialization.Patching.DSL.SyntaxTree
{
    public abstract class TransformationHeader
    {
        public abstract IEnumerable<TrackedSerializedData> Invoke( IEnumerable<TrackedSerializedData> pivot );
    }
}