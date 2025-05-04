
namespace UnityPlus.Serialization.Patching.DSL.SyntaxTree
{
    /// <summary>
    /// Represents any expression that can be evaluated against a pivot item to produce a result.
    /// </summary>
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