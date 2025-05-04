
namespace UnityPlus.Serialization.Patching.DSL.SyntaxTree
{
    /// <summary>
    /// A short-circuiting logical AND expression.
    /// </summary>
    public class ConditionalAndExpression : IExpression
    {
        public IExpression Left { get; set; }
        public IExpression Right { get; set; }

        public SerializedData Evaluate( TrackedSerializedData pivotItem )
        {
            // Short-circuiting logical AND.
            SerializedData left = Left.Evaluate( pivotItem );
            bool leftInput = (bool)OperationUtils.EnsureBoolean( left );

            if( leftInput == false )
                return (SerializedPrimitive)false;

            SerializedData right = Right.Evaluate( pivotItem );
            bool rightInput = (bool)OperationUtils.EnsureBoolean( right );

            if( rightInput == false )
                return (SerializedPrimitive)false;

            return (SerializedPrimitive)true;
        }
    }
}