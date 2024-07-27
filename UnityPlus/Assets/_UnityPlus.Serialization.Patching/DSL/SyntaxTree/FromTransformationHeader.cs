using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnityPlus.Serialization.Patching.DSL.SyntaxTree
{
    public class FromTransformationHeader : TransformationHeader
    {
        public SerializedDataPath Target { get; set; }

        public override IEnumerable<TrackedSerializedData> Invoke( IEnumerable<TrackedSerializedData> pivot )
        {
            IEnumerable<TrackedSerializedData> newPivots = Target.Evaluate( pivot );

            return newPivots;
        }
    }
}