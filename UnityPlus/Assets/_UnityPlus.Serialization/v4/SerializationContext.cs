
using System;
using System.Collections.Generic;

namespace UnityPlus.Serialization
{
    public interface ITypeResolver
    {
        Type ResolveType( string typeName );
    }

    public class DefaultTypeResolver : ITypeResolver
    {
        private readonly Dictionary<string, Type> _cache = new Dictionary<string, Type>();

        public Type ResolveType( string typeName )
        {
            if( _cache.TryGetValue( typeName, out var type ) )
                return type;

            type = Type.GetType( typeName );
            if( type != null )
                _cache[typeName] = type;

            return type;
        }
    }

    public class SerializationContext
    {
        public IForwardReferenceMap ForwardMap { get; set; }
        public IReverseReferenceMap ReverseMap { get; set; }
        public ITypeResolver TypeResolver { get; set; } = new DefaultTypeResolver();

        /// <summary>
        /// Collects errors, warnings, and info logs generated during the operation.
        /// </summary>
        public SerializationReport Report { get; } = new SerializationReport();

        /// <summary>
        /// If true, Collections (Arrays/Lists) are serialized as standard JSON arrays `[...]`.
        /// Note: This prevents circular references to arrays from being preserved.
        /// If false (default), Collections are wrapped in an object `{"$id":..., "values": [...]}` to ensure reference integrity.
        /// </summary>
        public bool ForceStandardJson { get; set; } = false;

        /// <summary>
        /// A global index of all ID'd data nodes available in the current context.
        /// Used for look-ahead when resolving references.
        /// </summary>
        public Dictionary<Guid, SerializedData> GlobalIdIndex { get; } = new Dictionary<Guid, SerializedData>();

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

        public void EnqueueDeferredRoot( ITypeDescriptor descriptor, SerializedData data )
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

    public struct DeferredOperation
    {
        public object Target;       // The Parent Object (or Parent Buffer). Null if this is a Root Object deferral.
        public IMemberInfo Member;  // The Member definition. Null if Root.
        public SerializedData Data; // The data required to build the child value
        public ITypeDescriptor Descriptor; // The descriptor for the value

        // State for resuming an interrupted construction
        public object[] ConstructionBuffer;
        public int ConstructionIndex;
    }
}
