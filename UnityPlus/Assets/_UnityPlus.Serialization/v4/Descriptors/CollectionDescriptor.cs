
using System;

namespace UnityPlus.Serialization
{
    public abstract class CollectionDescriptor : ICollectionTypeDescriptor
    {
        public abstract Type WrappedType { get; }

        public abstract object Resize( object target, int newSize );
        public abstract int GetStepCount( object target );
        public abstract IMemberInfo GetMemberInfo( int stepIndex, object target );
        public abstract object CreateInitialTarget( SerializedData data, SerializationContext ctx );

        public virtual int GetConstructionStepCount( object target ) => 0;
        public object Construct( object initialTarget ) => initialTarget;

        public virtual void OnSerializing( object target, SerializationContext context ) { }
        public virtual void OnDeserialized( object target, SerializationContext context ) { }

        public virtual int GetMethodCount() => 0;
        public virtual IMethodInfo GetMethodInfo( int methodIndex ) => throw new IndexOutOfRangeException();
    }
}
