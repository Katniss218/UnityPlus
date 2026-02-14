using System;
using System.Collections.Generic;
using System.Linq;

namespace UnityPlus.Serialization
{
    public class DictionaryDescriptor<TDict, TKey, TValue> : CollectionDescriptor where TDict : IDictionary<TKey, TValue>, new()
    {
        public override Type MappedType => typeof( TDict );

        public ContextKey KeyContext { get; set; } = ContextKey.Default;
        public ContextKey ValueContext { get; set; } = ContextKey.Default;

        private IDescriptor _cachedKvpDescriptor;

        public override object CreateInitialTarget( SerializedData data, SerializationContext ctx )
        {
            return new TDict();
        }

        public override object Resize( object target, int newSize )
        {
            return target;
        }

        public override int GetStepCount( object target )
        {
            return ((TDict)target).Count;
        }

        private IDescriptor GetKvpDescriptor()
        {
            if( _cachedKvpDescriptor == null )
            {
                _cachedKvpDescriptor = new KeyValuePairDescriptor( KeyContext, ValueContext );
            }
            return _cachedKvpDescriptor;
        }

        public override IEnumerator<IMemberInfo> GetMemberEnumerator( object target )
        {
            var dict = (TDict)target;
            var kvpDesc = GetKvpDescriptor();
            int index = 0;
            foreach( var kvp in dict )
            {
                yield return new DictionaryEntryMemberInfo( index++, kvp, kvpDesc, true );
            }
        }

        public override IMemberInfo GetMemberInfo( int stepIndex, object target )
        {
            var dict = (TDict)target;

            KeyValuePair<TKey, TValue> kvp = default;
            bool isExisting = stepIndex < dict.Count;
            if( isExisting )
            {
                kvp = dict.ElementAt( stepIndex );
            }

            return new DictionaryEntryMemberInfo( stepIndex, kvp, GetKvpDescriptor(), isExisting );
        }

        private struct DictionaryEntryMemberInfo : IMemberInfo
        {
            public readonly string Name => null;
            public readonly int Index => _index;
            public readonly Type MemberType => typeof( KeyValuePair<TKey, TValue> );
            public readonly bool RequiresWriteBack => true;
            public readonly IDescriptor TypeDescriptor => _descriptor;

            private int _index;
            private KeyValuePair<TKey, TValue> _kvp;
            private IDescriptor _descriptor;
            private bool _isExisting;

            public DictionaryEntryMemberInfo( int index, KeyValuePair<TKey, TValue> kvp, IDescriptor descriptor, bool isExisting )
            {
                _index = index;
                _kvp = kvp;
                _descriptor = descriptor;
                _isExisting = isExisting;
            }

            public object GetValue( object target )
            {
                return _isExisting ? (object)_kvp : null;
            }

            public void SetValue( ref object target, object value )
            {
                var dict = (TDict)target;
                var pair = (KeyValuePair<TKey, TValue>)value;

                if( pair.Key == null )
                    return;

                if( dict.ContainsKey( pair.Key ) )
                    dict[pair.Key] = pair.Value;
                else
                    dict.Add( pair.Key, pair.Value );
            }
        }

        private class KeyValuePairDescriptor : CompositeDescriptor
        {
            public override Type MappedType => typeof( KeyValuePair<TKey, TValue> );
            private readonly ContextKey _keyCtx;
            private readonly ContextKey _valCtx;

            private IDescriptor _keyDescriptor;
            private IDescriptor _valDescriptor;

            public KeyValuePairDescriptor( ContextKey keyCtx, ContextKey valCtx )
            {
                _keyCtx = keyCtx;
                _valCtx = valCtx;
                _keyDescriptor = TypeDescriptorRegistry.GetDescriptor( typeof( TKey ), _keyCtx );
                _valDescriptor = TypeDescriptorRegistry.GetDescriptor( typeof( TValue ), _valCtx );
            }

            public override int GetStepCount( object target ) => 2;
            public override int GetConstructionStepCount( object target ) => 2;

            public override IMemberInfo GetMemberInfo( int stepIndex, object target )
            {
                if( stepIndex == 0 ) return new KVPBufferMemberInfo( 0, "key", typeof( TKey ), _keyDescriptor );
                return new KVPBufferMemberInfo( 1, "value", typeof( TValue ), _valDescriptor );
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

            private struct KVPBufferMemberInfo : IMemberInfo
            {
                public string Name { get; }
                public readonly int Index => -1;
                public Type MemberType { get; }
                public readonly bool RequiresWriteBack => MemberType.IsValueType;
                public IDescriptor TypeDescriptor { get; }

                private int _index;

                public KVPBufferMemberInfo( int index, string name, Type type, IDescriptor desc )
                {
                    _index = index;
                    Name = name;
                    MemberType = type;
                    TypeDescriptor = desc;
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
    }
}