using System;
using System.Collections.Generic;

namespace UnityPlus.Serialization
{
    public class ListDescriptor<T> : CollectionDescriptor, ICollectionDescriptorWithContext
    {
        public override Type MappedType => typeof( List<T> );

        public ContextKey ElementContext { get; set; } = ContextKey.Default;

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

        public override IMemberInfo GetMemberInfo( int stepIndex, object target )
        {
            object element = ((List<T>)target)[stepIndex];
            Type actualType = element != null ? element.GetType() : typeof( T );
            if( typeof( T ).IsValueType || typeof( T ).IsSealed ) actualType = typeof( T );

            IDescriptor elementDesc = TypeDescriptorRegistry.GetDescriptor( actualType, ElementContext )
                                       ?? TypeDescriptorRegistry.GetDescriptor( typeof( T ), ElementContext );

            return new ListMemberInfo( stepIndex, elementDesc );
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