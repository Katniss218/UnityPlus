using System;
using System.Collections.Generic;

namespace UnityPlus.Serialization
{
#warning TODO - pretty sure this can be handled with a normal immutable memberwise descriptor. But first, we need to ensure that context selectors work with them. meaning, we need a selector in the memberwise descriptor.
    public class KeyValuePairDescriptor<TKey, TValue> : CompositeDescriptor
    {
        public override Type MappedType => typeof( KeyValuePair<TKey, TValue> );

        public IContextSelector ElementSelector { get; set; }

        public override int GetStepCount( object target ) => 2;
        public override int GetConstructionStepCount( object target ) => 2;

        public override IMemberInfo GetMemberInfo( int stepIndex )
        {
            return new KVPBufferMemberInfo( stepIndex, stepIndex == 0 ? "key" : "value", stepIndex == 0 ? typeof( TKey ) : typeof( TValue ), this );
        }

        public override object CreateInitialTarget( SerializedData data, SerializationContext ctx )
        {
            return new object[2];
        }

        public override object Construct( object initialTarget )
        {
            var buf = (object[])initialTarget;
            TKey k = buf[0] != null ? (TKey)buf[0] : default;
            TValue v = buf[1] != null ? (TValue)buf[1] : default;
            return new KeyValuePair<TKey, TValue>( k, v );
        }

        private readonly struct KVPBufferMemberInfo : IMemberInfo
        {
            public string Name { get; }
            public int Index => _index;
            public Type MemberType { get; }
            public bool RequiresWriteBack => MemberType.IsValueType;
            public IDescriptor TypeDescriptor => null;

            private readonly int _index;
            private readonly IContextSelector _parentSelector;

            public KVPBufferMemberInfo( int index, string name, Type type, KeyValuePairDescriptor<TKey, TValue> parentDescriptor )
            {
                _index = index;
                Name = name;
                MemberType = type;
                _parentSelector = parentDescriptor.ElementSelector;
            }

            public ContextKey GetContext( object target )
            {
#warning TODO - enable Uniform selector to work with keys.
                var args = new ContextSelectionArgs( _index, MemberType, MemberType, 2 );

                return _parentSelector.Select( args );
            }

            public object GetValue( object target )
            {
                if( target is KeyValuePair<TKey, TValue> pair )
                    return _index == 0 ? (object)pair.Key : pair.Value;

                if( target is object[] buf )
                    return buf[_index];

                return null;
            }

            public void SetValue( ref object target, object value )
            {
                if( target is object[] buf )
                {
                    buf[_index] = value;
                }
            }
        }
    }

#warning TODO
    public static class KeyValuePairDescriptorProvider
    {
        [MapsInheritingFrom( typeof( KeyValuePair<,> ) )]
        public static IDescriptor GetDescriptor<TKey, TValue>( ContextKey context )
        {
            var desc = new KeyValuePairDescriptor<TKey, TValue>()
            {
                ElementSelector = ContextRegistry.GetSelector( context )
            };

           /* desc = new ClassOrStructDescriptor<KeyValuePair<TKey, TValue>>()
            {
                ElementSelector = ContextRegistry.GetSelector( context )
            };*/

            return desc;
        }
    }
}