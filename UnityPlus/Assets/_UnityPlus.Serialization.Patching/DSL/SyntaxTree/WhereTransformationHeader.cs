using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnityPlus.Serialization.Patching.DSL.SyntaxTree
{
    public class WhereTransformationHeader : TransformationHeader
    {
        public BooleanExpression Filter { get; set; }

        public override IEnumerable<TrackedSerializedData> Invoke( IEnumerable<TrackedSerializedData> pivot )
        {
            IEnumerable<TrackedSerializedData> newPivots = pivot.Where( pivotItem => Filter.Evaluate( pivotItem ) );

            return newPivots;
        }
    }
}