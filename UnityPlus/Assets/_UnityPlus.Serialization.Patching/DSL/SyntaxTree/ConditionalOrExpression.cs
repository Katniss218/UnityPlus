
namespace UnityPlus.Serialization.Patching.DSL.SyntaxTree
{
    /// <summary>
    /// A short-circuiting logical OR expression.
    /// </summary>
    public class ConditionalOrExpression : IExpression
    {
        public IExpression Left { get; set; }
        public IExpression Right { get; set; }

        public SerializedData Evaluate( TrackedSerializedData pivotItem )
        {
            // Short-circuiting logical OR.
            SerializedData left = Left.Evaluate( pivotItem );
            bool leftInput = (bool)OperationUtils.EnsureBoolean( left );

            if( leftInput == true )
                return (SerializedPrimitive)true;

            SerializedData right = Right.Evaluate( pivotItem );
            bool rightInput = (bool)OperationUtils.EnsureBoolean( right );

            if( rightInput == true )
                return (SerializedPrimitive)true;

            return (SerializedPrimitive)false;
        }
    }
}