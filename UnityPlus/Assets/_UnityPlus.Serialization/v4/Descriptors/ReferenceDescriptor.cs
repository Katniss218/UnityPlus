
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

        public override DeserializeResult DeserializeDirect( SerializedData data, SerializationContext ctx, out object result )
        {
            result = null;
            if( data == null ) return DeserializeResult.Success;

            // 1. Extract GUID
            if( !data.TryGetValue( KeyNames.REF, out SerializedData refVal ) )
            {
                // Polymorphic mismatch or invalid data
                return DeserializeResult.Success; // Return null
            }

            if( !Guid.TryParse( (string)refVal, out Guid refGuid ) )
            {
                return DeserializeResult.Success; // Return null on bad guid
            }

            // 2. Check Loaded Objects (Hot Path)
            if( ctx.ForwardMap.TryGetObj( refGuid, out object existingObj ) )
            {
                result = existingObj;
                return DeserializeResult.Success;
            }

            // 3. Check Data Index (Circular Dependency / Deferred Resolution)
            if( ctx.GlobalIdIndex.ContainsKey( refGuid ) )
            {
                // The object exists in the file, but isn't loaded yet.
                return DeserializeResult.Deferred;
            }

            // 4. Truly Missing
            // We throw a specific exception here because this is likely data corruption, not just a load order issue.
            throw new UPSMissingReferenceException( $"Missing Reference: {refGuid}. It was not found in the ForwardMap nor the Loaded Data Index." );
        }
    }
}
