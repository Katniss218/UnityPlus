
using System;

namespace UnityPlus.Serialization
{
    public enum EnumSerializationMode
    {
        Integer,
        String
    }

    public class EnumDescriptor<T> : PrimitiveDescriptor<T> where T : struct, Enum
    {
        private readonly EnumSerializationMode _mode;

        public EnumDescriptor( EnumSerializationMode mode = EnumSerializationMode.Integer )
        {
            _mode = mode;
        }

        public override void SerializeDirect( object target, ref SerializedData data, SerializationContext ctx )
        {
            if( _mode == EnumSerializationMode.String )
            {
                data = (SerializedPrimitive)target.ToString();
            }
            else
            {
                // Box to int/byte/etc then to primitive
                data = (SerializedPrimitive)Convert.ToInt64( target );
            }
        }

        public override DeserializeResult DeserializeDirect( SerializedData data, SerializationContext ctx, out object result )
        {
            result = default( T );
            if( data is SerializedPrimitive prim )
            {
                // Reading String
                if( prim._type == SerializedPrimitive.DataType.String )
                {
                    if( Enum.TryParse<T>( (string)prim, true, out var res ) )
                    {
                        result = res;
                        return DeserializeResult.Success;
                    }
                }
                // Reading Number
                else if( prim._type == SerializedPrimitive.DataType.Int64 )
                {
                    result = Enum.ToObject( typeof( T ), (long)prim );
                    return DeserializeResult.Success;
                }
            }
            return DeserializeResult.Failed;
        }
    }
}
