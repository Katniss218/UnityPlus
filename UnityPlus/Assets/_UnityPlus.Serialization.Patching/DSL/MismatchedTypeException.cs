using System;
using System.Runtime.Serialization;

namespace UnityPlus.Serialization.Patching.DSL
{
    public class MismatchedTypeException : DSLExecutionException
    {
        public MismatchedTypeException()
        {
        }

        public MismatchedTypeException( string message )
            : base( message )
        {
        }

        public MismatchedTypeException( string message, Exception innerException )
            : base( message, innerException )
        {
        }

        protected MismatchedTypeException( SerializationInfo info, StreamingContext context )
            : base( info, context )
        {
        }
    }
}