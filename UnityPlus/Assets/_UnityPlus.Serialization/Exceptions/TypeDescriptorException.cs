
using System;
using System.Runtime.Serialization;

namespace UnityPlus.Serialization
{
    /// <summary>
    /// Thrown when an ITypeDescriptor is malformed, misconfigured, or behaves unexpectedly (e.g., Factory returns null).
    /// </summary>
    [Serializable]
    public class TypeDescriptorException : UPSSerializationException
    {
        /// <summary>
        /// The C# type that the descriptor was attempting to handle.
        /// </summary>
        public Type TargetType { get; }

        /// <summary>
        /// The specific descriptor instance, if available. 
        /// </summary>
        public ITypeDescriptor Descriptor { get; }

        public TypeDescriptorException()
        {
        }

        public TypeDescriptorException( string message )
            : base( message )
        {
        }

        public TypeDescriptorException( string message, Exception innerException )
            : base( message, innerException )
        {
        }

        /// <summary>
        /// Creates an exception associated with a specific type.
        /// </summary>
        public TypeDescriptorException( Type targetType, string message )
            : base( $"Descriptor Error [{targetType?.Name}]: {message}" )
        {
            TargetType = targetType;
        }

        /// <summary>
        /// Creates an exception associated with a specific type.
        /// </summary>
        public TypeDescriptorException( Type targetType, string message, Exception innerException )
            : base( $"Descriptor Error [{targetType?.Name}]: {message}", innerException )
        {
            TargetType = targetType;
        }

        /// <summary>
        /// Creates an exception associated with a specific descriptor instance.
        /// </summary>
        public TypeDescriptorException( ITypeDescriptor descriptor, string message )
            : base( $"Descriptor Error [{descriptor?.WrappedType?.Name ?? "Unknown"}]: {message}" )
        {
            Descriptor = descriptor;
            TargetType = descriptor?.WrappedType;
        }

        /// <summary>
        /// Creates an exception associated with a specific descriptor instance.
        /// </summary>
        public TypeDescriptorException( ITypeDescriptor descriptor, string message, Exception innerException )
            : base( $"Descriptor Error [{descriptor?.WrappedType?.Name ?? "Unknown"}]: {message}", innerException )
        {
            Descriptor = descriptor;
            TargetType = descriptor?.WrappedType;
        }

        protected TypeDescriptorException( SerializationInfo info, StreamingContext context )
            : base( info, context )
        {
            if( info != null )
            {
                string typeName = info.GetString( "TargetTypeName" );
                if( !string.IsNullOrEmpty( typeName ) )
                {
                    try { TargetType = Type.GetType( typeName ); } catch { }
                }
            }
        }

        public override void GetObjectData( SerializationInfo info, StreamingContext context )
        {
            base.GetObjectData( info, context );
            if( TargetType != null )
            {
                info.AddValue( "TargetTypeName", TargetType.AssemblyQualifiedName );
            }
        }
    }
}
