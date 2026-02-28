using System;

namespace UnityPlus.Serialization
{
    public class ArrayDescriptor<T> : CollectionDescriptor, ICollectionDescriptorWithContext
    {
        public override Type MappedType => typeof( T[] );

        public ContextKey ElementContext { get; set; } = ContextKey.Default;

        public override object CreateInitialTarget( SerializedData data, SerializationContext ctx )
        {
            int length = 0;

            var arr = SerializationHelpers.GetCollectionArrayNode( data );
            if( arr != null ) length = arr.Count;

            return new T[length];
        }

        public override object Resize( object target, int newSize )
        {
            // should be fine if 
            T[] array = (T[])target;
            if( array == null || array.Length != newSize )
            {
                if( array != null )
                    Array.Resize( ref array, newSize );
                else
                    array = new T[newSize];
            }
            return array;
        }

        public override int GetStepCount( object target )
        {
            return ((T[])target).Length;
        }

        private IDescriptor _cachedElementDescriptor;

        public override IMemberInfo GetMemberInfo( int stepIndex, object target )
        {
            if( _cachedElementDescriptor == null )
            {
                _cachedElementDescriptor = TypeDescriptorRegistry.GetDescriptor( typeof( T ), ElementContext );
            }

            return new ArrayMemberInfo( stepIndex, _cachedElementDescriptor );
        }

        private readonly struct ArrayMemberInfo : IMemberInfo
        {
            public string Name => null;
            public int Index => _index;
            public Type MemberType => typeof( T );
            public IDescriptor TypeDescriptor { get; }
            public bool RequiresWriteBack => typeof( T ).IsValueType;

            private readonly int _index;

            public ArrayMemberInfo( int index, IDescriptor descriptor )
            {
                _index = index;
                TypeDescriptor = descriptor;
            }

            public object GetValue( object target ) => ((T[])target)[_index];
            public void SetValue( ref object target, object value ) => ((T[])target)[_index] = (T)value;
        }
    }
}