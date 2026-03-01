using System;
using System.Collections.Generic;

namespace UnityPlus.Serialization
{
    public class ListDescriptor<T> : CollectionDescriptor, ICollectionDescriptorWithContext
    {
        public override Type MappedType => typeof( List<T> );

        public IContextSelector ElementSelector { get; set; }

        public override object CreateInitialTarget( SerializedData data, SerializationContext ctx )
        {
            int capacity = 0;
            var arr = SerializationHelpers.GetValueNode( data );
            if( arr != null ) capacity = arr.Count;

            return new List<T>( capacity );
        }

        public override object Resize( object target, int newSize )
        {
            List<T> list = (List<T>)target;
            list.Clear();

            if( list.Capacity < newSize )
            {
                list.Capacity = newSize;
            }

            for( int i = 0; i < newSize; i++ )
            {
                list.Add( default );
            }

            return list;
        }

        public override int GetStepCount( object target )
        {
            return ((List<T>)target).Count;
        }

        private IDescriptor _cachedElementDescriptor;

        public override IMemberInfo GetMemberInfo( int stepIndex, object target )
        {
            IDescriptor descriptor = null;

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
                var args = new ContextSelectionArgs( stepIndex, null, typeof( T ), null, null, ((List<T>)target).Count );
                var ctx = ElementSelector.Select( args );
                descriptor = TypeDescriptorRegistry.GetDescriptor( typeof( T ), ctx );
            }

            return new ListMemberInfo( stepIndex, descriptor );
        }

        private struct ListMemberInfo : IMemberInfo
        {
            public readonly string Name => null;
            public readonly int Index => _index;
            public readonly Type MemberType => typeof( T );
            public readonly IDescriptor TypeDescriptor { get; }
            public readonly bool RequiresWriteBack => typeof( T ).IsValueType;

            private int _index;

            public ListMemberInfo( int index, IDescriptor descriptor )
            {
                _index = index;
                TypeDescriptor = descriptor;
            }

            public object GetValue( object target ) => ((List<T>)target)[_index];
            public void SetValue( ref object target, object value ) => ((List<T>)target)[_index] = (T)value;
        }
    }
}