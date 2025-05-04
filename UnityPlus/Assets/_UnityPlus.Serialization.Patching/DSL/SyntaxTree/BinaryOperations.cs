namespace UnityPlus.Serialization.Patching.DSL.SyntaxTree
{
    public sealed class LessThanOp : IBinaryOperation
    {
        public SerializedData Evaluate( SerializedData left, SerializedData right )
        {
            if( left is null || right is null )
            {
                return false;
            }

            var leftInput = OperationUtils.EnsureNumber( left );
            var rightInput = OperationUtils.EnsureNumber( right );

            var leftType = leftInput.GetValueType();

            switch( leftType )
            {
                case SerializedPrimitive.DataType.Int64:
                    return (SerializedPrimitive)((long)leftInput < (long)rightInput);
                case SerializedPrimitive.DataType.UInt64:
                    return (SerializedPrimitive)((ulong)leftInput < (ulong)rightInput);
                case SerializedPrimitive.DataType.Float64:
                    return (SerializedPrimitive)((double)leftInput < (double)rightInput);
                case SerializedPrimitive.DataType.Decimal:
                    return (SerializedPrimitive)((decimal)leftInput < (decimal)rightInput);
                default:
                    throw new MismatchedTypeException();
            }
        }
    }

    public sealed class LessThanOrEqualOp : IBinaryOperation
    {
        public SerializedData Evaluate( SerializedData left, SerializedData right )
        {
            if( left is null || right is null )
            {
                return false;
            }

            var leftInput = OperationUtils.EnsureNumber( left );
            var rightInput = OperationUtils.EnsureNumber( right );

            var leftType = leftInput.GetValueType();

            switch( leftType )
            {
                case SerializedPrimitive.DataType.Int64:
                    return (SerializedPrimitive)((long)leftInput <= (long)rightInput);
                case SerializedPrimitive.DataType.UInt64:
                    return (SerializedPrimitive)((ulong)leftInput <= (ulong)rightInput);
                case SerializedPrimitive.DataType.Float64:
                    return (SerializedPrimitive)((double)leftInput <= (double)rightInput);
                case SerializedPrimitive.DataType.Decimal:
                    return (SerializedPrimitive)((decimal)leftInput <= (decimal)rightInput);
                default:
                    throw new MismatchedTypeException();
            }
        }
    }

    public sealed class EqualOp : IBinaryOperation
    {
        public SerializedData Evaluate( SerializedData left, SerializedData right )
        {
            if( left is null )
            {
                return right is null;
            }

            return (SerializedPrimitive)left.Equals( right );
        }
    }

    public sealed class NotEqualOp : IBinaryOperation
    {
        public SerializedData Evaluate( SerializedData left, SerializedData right )
        {
            if( left is null )
            {
                return right is not null;
            }

            return (SerializedPrimitive)(!left.Equals( right ));
        }
    }

    public sealed class GreaterThanOp : IBinaryOperation
    {
        public SerializedData Evaluate( SerializedData left, SerializedData right )
        {
            if( left is null || right is null )
            {
                return false;
            }

            var leftInput = OperationUtils.EnsureNumber( left );
            var rightInput = OperationUtils.EnsureNumber( right );

            var leftType = leftInput.GetValueType();

            switch( leftType )
            {
                case SerializedPrimitive.DataType.Int64:
                    return (SerializedPrimitive)((long)leftInput > (long)rightInput);
                case SerializedPrimitive.DataType.UInt64:
                    return (SerializedPrimitive)((ulong)leftInput > (ulong)rightInput);
                case SerializedPrimitive.DataType.Float64:
                    return (SerializedPrimitive)((double)leftInput > (double)rightInput);
                case SerializedPrimitive.DataType.Decimal:
                    return (SerializedPrimitive)((decimal)leftInput > (decimal)rightInput);
                default:
                    throw new MismatchedTypeException();
            }
        }
    }
    public sealed class GreaterThanOrEqualOp : IBinaryOperation
    {
        public SerializedData Evaluate( SerializedData left, SerializedData right )
        {
            if( left is null || right is null )
            {
                return false;
            }

            var leftInput = OperationUtils.EnsureNumber( left );
            var rightInput = OperationUtils.EnsureNumber( right );

            var leftType = leftInput.GetValueType();

            switch( leftType )
            {
                case SerializedPrimitive.DataType.Int64:
                    return (SerializedPrimitive)((long)leftInput >= (long)rightInput);
                case SerializedPrimitive.DataType.UInt64:
                    return (SerializedPrimitive)((ulong)leftInput >= (ulong)rightInput);
                case SerializedPrimitive.DataType.Float64:
                    return (SerializedPrimitive)((double)leftInput >= (double)rightInput);
                case SerializedPrimitive.DataType.Decimal:
                    return (SerializedPrimitive)((decimal)leftInput >= (decimal)rightInput);
                default:
                    throw new MismatchedTypeException();
            }
        }
    }

    public sealed class AddOp : IBinaryOperation
    {
        public SerializedData Evaluate( SerializedData left, SerializedData right )
        {
            var leftInput = OperationUtils.EnsureNumber( left );
            var rightInput = OperationUtils.EnsureNumber( right );

            var leftType = leftInput.GetValueType();

            var result = leftInput.GetValue();

            switch( leftType )
            {
                case SerializedPrimitive.DataType.Int64:
                    result.int64 += (long)rightInput; break;
                case SerializedPrimitive.DataType.UInt64:
                    result.uint64 += (ulong)rightInput; break;
                case SerializedPrimitive.DataType.Float64:
                    result.float64 += (double)rightInput; break;
                case SerializedPrimitive.DataType.Decimal:
                    result.@decimal += (decimal)rightInput; break;
                default:
                    throw new MismatchedTypeException();
            }

            return new SerializedPrimitive( result, leftType );
        }
    }

    public sealed class SubtractOp : IBinaryOperation
    {
        public SerializedData Evaluate( SerializedData left, SerializedData right )
        {
            var leftInput = OperationUtils.EnsureNumber( left );
            var rightInput = OperationUtils.EnsureNumber( right );

            var leftType = leftInput.GetValueType();

            var result = leftInput.GetValue();

            switch( leftType )
            {
                case SerializedPrimitive.DataType.Int64:
                    result.int64 -= (long)rightInput; break;
                case SerializedPrimitive.DataType.UInt64:
                    result.uint64 -= (ulong)rightInput; break;
                case SerializedPrimitive.DataType.Float64:
                    result.float64 -= (double)rightInput; break;
                case SerializedPrimitive.DataType.Decimal:
                    result.@decimal -= (decimal)rightInput; break;
                default:
                    throw new MismatchedTypeException();
            }

            return new SerializedPrimitive( result, leftType );
        }
    }

    public sealed class MultiplyOp : IBinaryOperation
    {
        public SerializedData Evaluate( SerializedData left, SerializedData right )
        {
            var leftInput = OperationUtils.EnsureNumber( left );
            var rightInput = OperationUtils.EnsureNumber( right );

            var leftType = leftInput.GetValueType();

            var result = leftInput.GetValue();

            switch( leftType )
            {
                case SerializedPrimitive.DataType.Int64:
                    result.int64 *= (long)rightInput; break;
                case SerializedPrimitive.DataType.UInt64:
                    result.uint64 *= (ulong)rightInput; break;
                case SerializedPrimitive.DataType.Float64:
                    result.float64 *= (double)rightInput; break;
                case SerializedPrimitive.DataType.Decimal:
                    result.@decimal *= (decimal)rightInput; break;
                default:
                    throw new MismatchedTypeException();
            }

            return new SerializedPrimitive( result, leftType );
        }
    }

    public sealed class DivideOp : IBinaryOperation
    {
        public SerializedData Evaluate( SerializedData left, SerializedData right )
        {
            var leftInput = OperationUtils.EnsureNumber( left );
            var rightInput = OperationUtils.EnsureNumber( right );

            var leftType = leftInput.GetValueType();

            var result = leftInput.GetValue();

            switch( leftType )
            {
                case SerializedPrimitive.DataType.Int64:
                    result.int64 /= (long)rightInput; break;
                case SerializedPrimitive.DataType.UInt64:
                    result.uint64 /= (ulong)rightInput; break;
                case SerializedPrimitive.DataType.Float64:
                    result.float64 /= (double)rightInput; break;
                case SerializedPrimitive.DataType.Decimal:
                    result.@decimal /= (decimal)rightInput; break;
                default:
                    throw new MismatchedTypeException();
            }

            return new SerializedPrimitive( result, leftType );
        }
    }

    public sealed class ModuloOp : IBinaryOperation
    {
        public SerializedData Evaluate( SerializedData left, SerializedData right )
        {
            var leftInput = OperationUtils.EnsureNumber( left );
            var rightInput = OperationUtils.EnsureNumber( right );

            var leftType = leftInput.GetValueType();

            var result = leftInput.GetValue();

            switch( leftType )
            {
                case SerializedPrimitive.DataType.Int64:
                    result.int64 %= (long)rightInput; break;
                case SerializedPrimitive.DataType.UInt64:
                    result.uint64 %= (ulong)rightInput; break;
                case SerializedPrimitive.DataType.Float64:
                    result.float64 %= (double)rightInput; break;
                case SerializedPrimitive.DataType.Decimal:
                    result.@decimal %= (decimal)rightInput; break;
                default:
                    throw new MismatchedTypeException();
            }

            return new SerializedPrimitive( result, leftType );
        }
    }

    public sealed class LogicalAndOp : IBinaryOperation
    {
        public SerializedData Evaluate( SerializedData left, SerializedData right )
        {
            var leftInput = OperationUtils.EnsureBooleanOrInteger( left );
            var rightInput = OperationUtils.EnsureBooleanOrInteger( right );

            var leftType = leftInput.GetValueType();

            switch( leftType )
            {
                case SerializedPrimitive.DataType.Int64:
                    return (SerializedPrimitive)((long)leftInput & (long)rightInput);
                case SerializedPrimitive.DataType.UInt64:
                    return (SerializedPrimitive)((ulong)leftInput & (ulong)rightInput);
                case SerializedPrimitive.DataType.Boolean:
                    return (SerializedPrimitive)((bool)leftInput & (bool)rightInput);
                default:
                    throw new MismatchedTypeException();
            }
        }
    }

    public sealed class LogicalOrOp : IBinaryOperation
    {
        public SerializedData Evaluate( SerializedData left, SerializedData right )
        {
            var leftInput = OperationUtils.EnsureBooleanOrInteger( left );
            var rightInput = OperationUtils.EnsureBooleanOrInteger( right );

            var leftType = leftInput.GetValueType();

            switch( leftType )
            {
                case SerializedPrimitive.DataType.Int64:
                    return (SerializedPrimitive)((long)leftInput | (long)rightInput);
                case SerializedPrimitive.DataType.UInt64:
                    return (SerializedPrimitive)((ulong)leftInput | (ulong)rightInput);
                case SerializedPrimitive.DataType.Boolean:
                    return (SerializedPrimitive)((bool)leftInput | (bool)rightInput);
                default:
                    throw new MismatchedTypeException();
            }
        }
    }

    public sealed class LogicalXorOp : IBinaryOperation
    {
        public SerializedData Evaluate( SerializedData left, SerializedData right )
        {
            var leftInput = OperationUtils.EnsureBooleanOrInteger( left );
            var rightInput = OperationUtils.EnsureBooleanOrInteger( right );

            var leftType = leftInput.GetValueType();

            switch( leftType )
            {
                case SerializedPrimitive.DataType.Int64:
                    return (SerializedPrimitive)((long)leftInput ^ (long)rightInput);
                case SerializedPrimitive.DataType.UInt64:
                    return (SerializedPrimitive)((ulong)leftInput ^ (ulong)rightInput);
                case SerializedPrimitive.DataType.Boolean:
                    return (SerializedPrimitive)((bool)leftInput ^ (bool)rightInput);
                default:
                    throw new MismatchedTypeException();
            }
        }
    }
}