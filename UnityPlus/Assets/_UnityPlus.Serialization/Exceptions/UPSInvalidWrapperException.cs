using System;

namespace UnityPlus.Serialization
{
    public class UPSInvalidWrapperException : UPSSerializationException
    {
        public UPSInvalidWrapperException( IReadonlySerializationContext ctx, string message, string path, IDescriptor descriptor, IMemberInfo member, string operation, Exception innerException )
           : base( ctx, message, path, descriptor, member, operation, innerException )
        {
        }
    }
    public class UPSCircularReferenceException : UPSSerializationException
    {
        public UPSCircularReferenceException( IReadonlySerializationContext ctx, string message, string path, IDescriptor descriptor, IMemberInfo member, string operation, Exception innerException )
           : base( ctx, message, path, descriptor, member, operation, innerException )
        {
        }
    }
}