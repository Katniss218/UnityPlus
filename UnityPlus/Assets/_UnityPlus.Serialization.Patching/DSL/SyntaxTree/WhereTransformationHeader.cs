using System.Collections.Generic;
using System.Linq;

namespace UnityPlus.Serialization.Patching.DSL.SyntaxTree
{
    /// <summary>
    /// Represents the 'WHERE' transformation header.
    /// </summary>
    /// <example>
    /// 
    ///     (WHERE "$type" == "somevalue")
    ///     {
    ///         // ...
    ///     }
    /// 
    /// </example>
    public class WhereTransformationHeader : TransformationHeader
    {
        public IExpression Filter { get; set; }

        public override IEnumerable<TrackedSerializedData> Invoke( IEnumerable<TrackedSerializedData> pivot )
        {
            IEnumerable<TrackedSerializedData> newPivots = pivot.Where( pivotItem =>
            {
                var result = Filter.Evaluate( pivotItem );

                // Anything that is not a primitive boolean is falsy. TODO - may not be the best but whatever.
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