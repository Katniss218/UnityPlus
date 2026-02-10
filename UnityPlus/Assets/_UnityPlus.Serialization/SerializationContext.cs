using System;
using System.Collections.Generic;

namespace UnityPlus.Serialization
{
    public class SerializationContext
    {
        public IForwardReferenceMap ForwardMap { get; set; } // Used for serialization.
        public IReverseReferenceMap ReverseMap { get; set; } // Used for deserialization.

        public ITypeResolver TypeResolver { get; set; } = new DefaultTypeResolver();

        /// <summary>
        /// Collects errors, warnings, and info logs generated during the operation.
        /// </summary>
        public SerializationLog Log { get; } = new SerializationLog();

        /// <summary>
        /// If true, Collections (Arrays/Lists) are serialized as standard JSON arrays `[...]`.
        /// If false (default), Collections are wrapped in an object `{"$id":..., "values": [...]}` to ensure reference integrity.
        /// </summary>
        /// <remarks>
        /// This prevents references to arrays from being preserved, but allows compatibility with standard json syntax.
        /// </remarks>
        public bool ForceStandardJson { get; set; } = false;

        /// <summary>
        /// Holds operations that failed due to missing dependencies.
        /// They will be retried after the main stack is cleared.
        /// </summary>
        public Queue<DeferredOperation> DeferredOperations { get; private set; } = new Queue<DeferredOperation>();

        public void EnqueueDeferred( object target, IMemberInfo member, SerializedData data )
        {
            DeferredOperations.Enqueue( new DeferredOperation
            {
                Target = target,
                Member = member,
                Data = data,
                Descriptor = member.TypeDescriptor
            } );
        }

        public void EnqueueDeferredRoot( IDescriptor descriptor, SerializedData data )
        {
            DeferredOperations.Enqueue( new DeferredOperation
            {
                Target = null,
                Member = null,
                Data = data,
                Descriptor = descriptor
            } );
        }
    }
}