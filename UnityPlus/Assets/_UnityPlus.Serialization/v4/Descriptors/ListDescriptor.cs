
using System;
using System.Collections.Generic;

namespace UnityPlus.Serialization
{
    public class ListDescriptor<T> : CollectionDescriptor, TypeDescriptorRegistry.ICollectionDescriptorWithContext
    {
        public override Type WrappedType => typeof( List<T> );

        // The context used to resolve descriptors for the elements of the list.
        public int ElementContext { get; set; } = 0;

        public override object CreateInitialTarget( SerializedData data, SerializationContext ctx )
        {
            int capacity = 0;
            var arr = SerializationHelpers.GetCollectionArrayNode( data );
            if( arr != null ) capacity = arr.Count;

            return new List<T>( capacity );
        }

        public override object Resize( object target, int newSize )
        {
            List<T> list = (List<T>)target;
            if( list.Count < newSize )
            {
                int itemsToAdd = newSize - list.Count;
                for( int i = 0; i < itemsToAdd; i++ ) list.Add( default );
            }
            else if( list.Count > newSize )
            {
                list.RemoveRange( newSize, list.Count - newSize );
            }
            return list;
        }

        public override int GetStepCount( object target )
        {
            return ((List<T>)target).Count;
        }

        public override IMemberInfo GetMemberInfo( int stepIndex, object target )
        {
            object element = ((List<T>)target)[stepIndex];
            Type actualType = element != null ? element.GetType() : typeof( T );
            if( typeof( T ).IsValueType || typeof( T ).IsSealed ) actualType = typeof( T );

            // Use ElementContext to resolve the element descriptor
            ITypeDescriptor elementDesc = TypeDescriptorRegistry.GetDescriptor( actualType, ElementContext )
                                       ?? TypeDescriptorRegistry.GetDescriptor( typeof( T ), ElementContext );

            return new ListMemberInfo( stepIndex, elementDesc );
        }

        private struct ListMemberInfo : IMemberInfo
        {
            public string Name => null;
            public int Index => _index;
            public Type MemberType => typeof( T );
            public ITypeDescriptor TypeDescriptor { get; }
            public bool IsValueType => typeof( T ).IsValueType;

            private int _index;

            public ListMemberInfo( int index, ITypeDescriptor descriptor )
            {
                _index = index;
                TypeDescriptor = descriptor;
            }

            public object GetValue( object target ) => ((List<T>)target)[_index];
            public void SetValue( ref object target, object value ) => ((List<T>)target)[_index] = (T)value;
        }
    }
}
