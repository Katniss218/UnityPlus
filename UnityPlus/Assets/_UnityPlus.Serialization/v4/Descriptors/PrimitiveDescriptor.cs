
using System;

namespace UnityPlus.Serialization
{
    public abstract class PrimitiveDescriptor<T> : IPrimitiveTypeDescriptor
    {
        public Type WrappedType => typeof( T );

        public abstract void SerializeDirect( object target, ref SerializedData data, SerializationContext ctx );
        public abstract DeserializeResult DeserializeDirect( SerializedData data, SerializationContext ctx, out object result );

        public object CreateInitialTarget( SerializedData data, SerializationContext ctx ) => default( T );
    }

    public class IntDescriptor : PrimitiveDescriptor<int>
    {
        public override void SerializeDirect( object target, ref SerializedData data, SerializationContext ctx ) => data = (SerializedPrimitive)(int)target;
        public override DeserializeResult DeserializeDirect( SerializedData data, SerializationContext ctx, out object result )
        {
            result = (int)data;
            return DeserializeResult.Success;
        }
    }

    public class FloatDescriptor : PrimitiveDescriptor<float>
    {
        public override void SerializeDirect( object target, ref SerializedData data, SerializationContext ctx ) => data = (SerializedPrimitive)(float)target;
        public override DeserializeResult DeserializeDirect( SerializedData data, SerializationContext ctx, out object result )
        {
            result = (float)data;
            return DeserializeResult.Success;
        }
    }

    public class StringDescriptor : PrimitiveDescriptor<string>
    {
        public override void SerializeDirect( object target, ref SerializedData data, SerializationContext ctx ) => data = (SerializedPrimitive)(string)target;
        public override DeserializeResult DeserializeDirect( SerializedData data, SerializationContext ctx, out object result )
        {
            result = (string)data;
            return DeserializeResult.Success;
        }
    }

    public class BoolDescriptor : PrimitiveDescriptor<bool>
    {
        public override void SerializeDirect( object target, ref SerializedData data, SerializationContext ctx ) => data = (SerializedPrimitive)(bool)target;
        public override DeserializeResult DeserializeDirect( SerializedData data, SerializationContext ctx, out object result )
        {
            result = (bool)data;
            return DeserializeResult.Success;
        }
    }
}
