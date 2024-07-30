using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace UnityPlus.Serialization.Patching.DSL.SyntaxTree
{
    internal static class OperationUtils
    {
        internal static SerializedPrimitive EnsureBoolean( SerializedData value )
        {
            if( value is not SerializedPrimitive prim )
            {
                throw new MismatchedTypeException();
            }
            if( prim.GetValueType() != SerializedPrimitive.DataType.Boolean )
            {
                throw new MismatchedTypeException();
            }
            return prim;
        }

        internal static SerializedPrimitive EnsureNumber( SerializedData value )
        {
            if( value is not SerializedPrimitive prim )
            {
                throw new MismatchedTypeException();
            }
            if( prim.GetValueType() != SerializedPrimitive.DataType.Int64
             && prim.GetValueType() != SerializedPrimitive.DataType.UInt64
             && prim.GetValueType() != SerializedPrimitive.DataType.Float64
             && prim.GetValueType() != SerializedPrimitive.DataType.Decimal )
            {
                throw new MismatchedTypeException();
            }
            return prim;
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

    public sealed class AndOp : IBinaryOperation
    {
        public SerializedData Evaluate( SerializedData left, SerializedData right )
        {
            // Short-circuiting logical AND.
            bool leftInput = (bool)OperationUtils.EnsureBoolean( left );

            if( leftInput == false )
                return (SerializedPrimitive)false;

            bool rightInput = (bool)OperationUtils.EnsureBoolean( right );

            if( rightInput == false )
                return (SerializedPrimitive)false;

            return (SerializedPrimitive)true;
        }
    }

    public sealed class OrOp : IBinaryOperation
    {
        public SerializedData Evaluate( SerializedData left, SerializedData right )
        {
            // Short-circuiting logical OR.
            bool leftInput = (bool)OperationUtils.EnsureBoolean( left );

            if( leftInput == true )
                return (SerializedPrimitive)true;

            bool rightInput = (bool)OperationUtils.EnsureBoolean( right );

            if( rightInput == true )
                return (SerializedPrimitive)true;

            return (SerializedPrimitive)false;
        }
    }

    public sealed class AddOp : IBinaryOperation
    {
        public SerializedData Evaluate( SerializedData left, SerializedData right )
        {
            var leftInput = OperationUtils.EnsureNumber( left );
            var leftType = leftInput.GetValueType();

            var rightInput = OperationUtils.EnsureBoolean( right );

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
            var leftType = leftInput.GetValueType();

            var rightInput = OperationUtils.EnsureBoolean( right );

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
            var leftType = leftInput.GetValueType();

            var rightInput = OperationUtils.EnsureBoolean( right );

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
            var leftType = leftInput.GetValueType();

            var rightInput = OperationUtils.EnsureBoolean( right );

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
            var leftType = leftInput.GetValueType();

            var rightInput = OperationUtils.EnsureBoolean( right );

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

    public sealed class NotOp : IUnaryOperation
    {
        public SerializedData Evaluate( SerializedData value )
        {
            bool input = OperationUtils.EnsureBoolean( value );

            bool output = !input;
            return (SerializedPrimitive)output;
        }
    }
}