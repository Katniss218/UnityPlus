using System.Collections.Generic;

namespace UnityPlus.Serialization.Patching.DSL.SyntaxTree
{
    public interface IStatement
    {
        void Invoke( IEnumerable<TrackedSerializedData> pivot );
    }
}