namespace UnityPlus.Serialization.Patching.DSL.SyntaxTree
{
    public sealed class NotOp : IUnaryOperation
    {
        public SerializedData Evaluate( SerializedData value )
        {
            bool input = OperationUtils.EnsureBoolean( value );

            bool output = !input;
            return (SerializedPrimitive)output;
        }
    }

    public sealed class NegateOp : IUnaryOperation
    {
        public SerializedData Evaluate( SerializedData value )
        {
            SerializedPrimitive value2 = OperationUtils.EnsureSignedNumber( value );

            switch( value2.GetValueType() )
            {
                case SerializedPrimitive.DataType.Int64:
                    return (SerializedPrimitive)(-(long)value2);
                case SerializedPrimitive.DataType.Float64:
                    return (SerializedPrimitive)(-(double)value2);
                case SerializedPrimitive.DataType.Decimal:
                    return (SerializedPrimitive)(-(decimal)value2);
                default:
                    throw new MismatchedTypeException();
            }
        }
    }
}