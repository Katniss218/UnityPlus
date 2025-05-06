
namespace UnityPlus.Serialization.Patching.DSL.SyntaxTree
{
    public interface IUnaryOperation
    {
        SerializedData Evaluate( SerializedData value );
    }

    public class UnaryExpression : IExpression
    {
        public IExpression Center { get; set; }

        public IUnaryOperation Op { get; set; }

        public SerializedData Evaluate( TrackedSerializedData pivotItem )
        {
            SerializedData center = Center.Evaluate( pivotItem );

            SerializedData result = Op.Evaluate( center );
            return result;
        }
    }
}