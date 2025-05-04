using System.Collections.Generic;
using System.Linq;

namespace UnityPlus.Serialization.Patching.DSL.SyntaxTree
{
    /// <summary>
    /// The '$' operator. Used to retrieve a value from the pivot.
    /// </summary>
    public sealed class ValueOfExpression : IExpression
    {
        public SerializedDataPath Target { get; set; }

        public SerializedData Evaluate( TrackedSerializedData pivotItem )
        {
            IEnumerable<TrackedSerializedData> newPivots = Target.Evaluate( pivotItem );
            if( newPivots == null || newPivots.Count() != 1 )
                throw new DSLExecutionException( "the '$' operator of can return only 1 element." );

            return newPivots.First().value;
        }
    }
}