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
            if( arr != null ) 
                length = arr.Count;

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

#warning TODO - finish.
        private IDescriptor _cachedElementDescriptor;

        public override IMemberInfo GetMemberInfo( int stepIndex )
        {
            return new ArrayMemberInfo( stepIndex, ElementSelector );
        }

        private readonly struct ArrayMemberInfo : IMemberInfo
        {
            public string Name => null;
            public int Index => _index;
            public Type MemberType => typeof( T );
            public bool RequiresWriteBack => typeof( T ).IsValueType;

            private readonly int _index;
            private readonly IContextSelector _selector;

            public ArrayMemberInfo( int index, IContextSelector selector )
            {
                _index = index;
                _selector = selector;
            }

            public ContextKey GetContext( object target )
            {
                if( _selector is UniformSelector uniform )
                    return uniform.Select( default );

                var args = new ContextSelectionArgs( _index, typeof( T ), typeof( T ), ((T[])target).Length );
                return _selector.Select( args );
            }

            public IDescriptor TypeDescriptor
            {
                get
                {
                    if( _selector is UniformSelector uniform )
                        return TypeDescriptorRegistry.GetDescriptor( typeof( T ), uniform.Select( default ) );
                    return null; // Dynamic resolution by strategy
                }
            }

            public object GetValue( object target ) => ((T[])target)[_index];
            public void SetValue( ref object target, object value ) => ((T[])target)[_index] = (T)value;
        }
    }
}