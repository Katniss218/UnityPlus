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
    public class ForTransformationHeader : TransformationHeader
    {
        public SerializedDataPath Target { get; set; }

        public override IEnumerable<TrackedSerializedData> Invoke( IEnumerable<TrackedSerializedData> pivot )
        {
            IEnumerable<TrackedSerializedData> newPivots = Target.Evaluate( pivot );

            return newPivots;
        }
    }
}