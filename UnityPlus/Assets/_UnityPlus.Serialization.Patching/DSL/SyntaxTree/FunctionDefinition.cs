namespace UnityPlus.Serialization.Patching.DSL.SyntaxTree
{
    public abstract class FunctionDefinition
    {
        public abstract int ArgumentCount { get; }

        public abstract SerializedData Invoke( SerializedData[] arguments ); // ref params will require swapping SerializedData[] for TrackedSerializedData[]
    }

    public sealed class FunctionDefinition_Contains : FunctionDefinition
    {
        public override int ArgumentCount => 2;

        public override SerializedData Invoke( SerializedData[] arguments )
        {
            if( arguments.Length != ArgumentCount )
            {
                throw new DSLExecutionException( $"Function '{nameof( FunctionDefinition_Contains )}' requires {ArgumentCount} arguments." );
            }

            if( arguments[0] is SerializedPrimitive p1 && arguments[1] is SerializedPrimitive p2
             && p1.GetValueType() == SerializedPrimitive.DataType.String && p2.GetValueType() == SerializedPrimitive.DataType.String )
            {
                return p1.GetValue().str.Contains( p2.GetValue().str );
            }

            throw new DSLExecutionException( $"Invalid combination of arguments '{nameof( FunctionDefinition_Contains )}'." );
        }
    }
}