using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnityPlus.Serialization.Patching.DSL.SyntaxTree
{
    public class WhereTransformationHeader : TransformationHeader
    {
        public IExpression Filter { get; set; }

        public override IEnumerable<TrackedSerializedData> Invoke( IEnumerable<TrackedSerializedData> pivot )
        {
            IEnumerable<TrackedSerializedData> newPivots = pivot.Where( pivotItem =>
            {
                var result = Filter.Evaluate( pivotItem );

                if( result is not SerializedPrimitive prim )
                    return false;

                if( prim.GetValueType() != SerializedPrimitive.DataType.Boolean )
                    return false;

                return prim.GetValue().boolean;
            } );

            return newPivots;
        }
    }
}