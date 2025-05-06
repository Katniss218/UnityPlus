using System.Collections.Generic;

namespace UnityPlus.Serialization.Patching.DSL.SyntaxTree
{
    public interface ITransformationHeader
    {
        public IEnumerable<TrackedSerializedData> Invoke( IEnumerable<TrackedSerializedData> pivot );
    }
}