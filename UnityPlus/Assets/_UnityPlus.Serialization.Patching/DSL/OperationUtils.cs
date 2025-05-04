namespace UnityPlus.Serialization.Patching.DSL
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

        internal static SerializedPrimitive EnsureSignedNumber( SerializedData value )
        {
            if( value is not SerializedPrimitive prim )
            {
                throw new MismatchedTypeException();
            }
            if( prim.GetValueType() != SerializedPrimitive.DataType.Int64
             && prim.GetValueType() != SerializedPrimitive.DataType.Float64
             && prim.GetValueType() != SerializedPrimitive.DataType.Decimal )
            {
                throw new MismatchedTypeException();
            }
            return prim;
        }

        internal static SerializedPrimitive EnsureBooleanOrInteger( SerializedData value )
        {
            if( value is not SerializedPrimitive prim )
            {
                throw new MismatchedTypeException();
            }
            if( prim.GetValueType() != SerializedPrimitive.DataType.Boolean
             && prim.GetValueType() != SerializedPrimitive.DataType.Int64
             && prim.GetValueType() != SerializedPrimitive.DataType.UInt64 )
            {
                throw new MismatchedTypeException();
            }
            return prim;
        }

        internal static SerializedPrimitive EnsureString( SerializedData value )
        {
            if( value is not SerializedPrimitive prim )
            {
                throw new MismatchedTypeException();
            }
            if( prim.GetValueType() != SerializedPrimitive.DataType.String )
            {
                throw new MismatchedTypeException();
            }
            return prim;
        }
    }
}