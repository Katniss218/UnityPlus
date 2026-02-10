using System;

namespace UnityPlus.Serialization
{
    /// <summary>
    /// Serializes an object as a reference ("$ref") using the ReferenceMap.
    /// Used when the member context is set to ObjectContext.Ref.
    /// </summary>
    [TypeDescriptorContext( ObjectContext.Ref )]
    public class ReferenceDescriptor<T> : PrimitiveDescriptor<T> where T : class
    {
        public override void SerializeDirect( object target, ref SerializedData data, SerializationContext ctx )
        {
            data = ctx.ReverseMap.WriteObjectReference( (T)target );
        }

        public override DeserializationResult DeserializeDirect( SerializedData data, SerializationContext ctx, out object result )
        {
            result = null;
            if( data == null ) return DeserializationResult.Success;

            // 1. Extract GUID
#warning TODO - use v3 extension for guid loading.
            if( !data.TryGetValue( KeyNames.REF, out SerializedData refVal ) )
            {
                // Polymorphic mismatch or invalid data
                return DeserializationResult.Success; // Return null
            }

            if( !Guid.TryParse( (string)refVal, out Guid refGuid ) )
            {
                return DeserializationResult.Success; // Return null on bad guid
            }

            // 2. Check Loaded Objects (Hot Path)
            if( ctx.ForwardMap.TryGetObj( refGuid, out object existingObj ) )
            {
                result = existingObj;
                return DeserializationResult.Success;
            }

            // 3. Optimistic Deferral
            // We assume the object will be loaded later in the process.
            // If it is truly missing, the Driver will flag it as a deadlock at the end of the operation.
            return DeserializationResult.Deferred;
        }
    }
}