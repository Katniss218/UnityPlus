
namespace UnityPlus.Serialization.Patching.DSL.SyntaxTree
{
    public interface IExpression
    {
        /// <summary>
        /// Evaluates the expression against the provided pivot item and returns the result.
        /// </summary>
        /// <remarks>
        /// An expression can only return a single value.
        /// </remarks>
        SerializedData Evaluate( TrackedSerializedData pivotItem );
    }
}