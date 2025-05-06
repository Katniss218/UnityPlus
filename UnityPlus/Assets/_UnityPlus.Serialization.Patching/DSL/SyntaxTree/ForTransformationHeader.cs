using System.Collections.Generic;

namespace UnityPlus.Serialization.Patching.DSL.SyntaxTree
{
    /// <summary>
    /// Represents the 'FOR' transformation header.
    /// </summary>
    /// <example>
    /// 
    ///     (FOR vessels[*].gameobjects)
    ///     {
    ///         // ...
    ///     }
    /// 
    /// </example>
    public class ForTransformationHeader : ITransformationHeader
    {
        public SerializedDataPath Target { get; set; }

        public IEnumerable<TrackedSerializedData> Invoke( IEnumerable<TrackedSerializedData> pivot )
        {
            IEnumerable<TrackedSerializedData> newPivots = Target.Evaluate( pivot );

            return newPivots;
        }
    }
}