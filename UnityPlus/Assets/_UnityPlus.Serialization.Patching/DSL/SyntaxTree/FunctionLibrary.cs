using System.Collections.Generic;

namespace UnityPlus.Serialization.Patching.DSL.SyntaxTree
{
    public static class FunctionLibrary
    {
        private static readonly Dictionary<string, FunctionDefinition> functionDefinitions = new Dictionary<string, FunctionDefinition>
        {
            { "Contains", new FunctionDefinition_Contains() }
        };

        public static SerializedData Invoke( string functionName, SerializedData[] arguments )
        {
            if( functionDefinitions.TryGetValue( functionName, out FunctionDefinition function ) )
            {
                if( arguments.Length != function.ArgumentCount )
                {
                    throw new DSLExecutionException( $"Function '{functionName}' requires {function.ArgumentCount} arguments." );
                }

                return function.Invoke( arguments );
            }

            throw new DSLExecutionException( $"Function '{functionName}' not found." );
        }
    }
}