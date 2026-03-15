
using System;

namespace UnityPlus.Serialization
{
    public abstract class PrimitiveDescriptor<T> : IPrimitiveDescriptor
    {
        public Type MappedType => typeof( T );

        public abstract void SerializeDirect( object target, ref SerializedData data, SerializationContext ctx );
        public abstract DeserializationResult DeserializeDirect( SerializedData data, SerializationContext ctx, out object result );

        public virtual object CreateInitialTarget( SerializedData data, SerializationContext ctx ) => default( T );
        public virtual ObjectStructure DetermineObjectStructure( Type declaredType, Type actualType, SerializationConfiguration config, out bool needsId, out bool needsType )
        {
            SerializationHelpers.DetermineObjectStructure( declaredType, actualType, out needsId, out needsType );

            if( (needsId || needsType) && !config.ForceStandardJson )
            {
                return ObjectStructure.Wrapped;
            }
            return ObjectStructure.Unwrapped;
        }
    }

    /// <summary>
    /// A primitive descriptor that delegates logic to functions.
    /// Useful for defining serialization logic inline within factory methods.
    /// </summary>
    public class PrimitiveConfigurableDescriptor<T> : PrimitiveDescriptor<T>
    {
        private readonly Action<T, SerializedDataWrapper, SerializationContext> _serializer;
        private readonly Func<SerializedData, SerializationContext, T> _deserializer;

        /// <summary>
        /// Wrapper to allow setting the ref parameter inside the lambda.
        /// </summary>
        public class SerializedDataWrapper
        {
            public SerializedData Data;
        }

        public PrimitiveConfigurableDescriptor( Action<T, SerializedDataWrapper, SerializationContext> serializer, Func<SerializedData, SerializationContext, T> deserializer )
        {
            _serializer = serializer;
            _deserializer = deserializer;
        }

        public override void SerializeDirect( object target, ref SerializedData data, SerializationContext ctx )
        {
            var wrapper = new SerializedDataWrapper { Data = data };
            _serializer( (T)target, wrapper, ctx );
            data = wrapper.Data;
        }

        public override DeserializationResult DeserializeDirect( SerializedData data, SerializationContext ctx, out object result )
        {
            try
            {
                result = _deserializer( data, ctx );
                return DeserializationResult.Success;
            }
            catch
            {
                result = default( T );
                return DeserializationResult.Failed;
            }
        }
    }
}