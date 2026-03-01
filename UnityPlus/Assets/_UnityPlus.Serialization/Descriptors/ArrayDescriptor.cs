using System;

namespace UnityPlus.Serialization
{
    public class ArrayDescriptor<T> : CollectionDescriptor, ICollectionDescriptorWithContext
    {
        public override Type MappedType => typeof( T[] );

        public IContextSelector ElementSelector { get; set; }

        public override object CreateInitialTarget( SerializedData data, SerializationContext ctx )
        {
            int length = 0;

            var arr = SerializationHelpers.GetValueNode( data );
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
            IDescriptor descriptor;
            if( ElementSelector is UniformSelector uniform )
            {
                if( _cachedElementDescriptor == null )
                {
                    var ctx = uniform.Select( default );
                    _cachedElementDescriptor = TypeDescriptorRegistry.GetDescriptor( typeof( T ), ctx );
                }
                descriptor = _cachedElementDescriptor;
            }
            else
            {
#warning TODO - needs to get the actual type and serializeddata, somehow. 
                // so we need to invert the logic, to scan the serialized data for the element type, and then use that to select the context, and then get the appropriate descriptor only once.
                // so, instead of assigning the actual descriptor to the memberInfo, we want to assign the ContextKey.

                var args = new ContextSelectionArgs( stepIndex, null, typeof( T ), null, null, ((T[])target).Length );
                var ctx = ElementSelector.Select( args );
                descriptor = TypeDescriptorRegistry.GetDescriptor( typeof( T ), ctx );
            }

            return new ArrayMemberInfo( stepIndex, descriptor );
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