using System;
using System.Runtime.Serialization;

namespace UnityPlus.Serialization.Patching.DSL
{
    public class DSLExecutionException : Exception
    {
        public DSLExecutionException()
        {
        }

        public DSLExecutionException( string message )
            : base( message )
        {
        }

        public DSLExecutionException( string message, Exception innerException )
            : base( message, innerException )
        {
        }

        protected DSLExecutionException( SerializationInfo info, StreamingContext context )
            : base( info, context )
        {
        }
    }
}