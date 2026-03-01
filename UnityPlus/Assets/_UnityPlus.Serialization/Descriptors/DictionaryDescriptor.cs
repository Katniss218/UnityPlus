using System;
using System.Collections.Generic;
using System.Linq;

namespace UnityPlus.Serialization
{
    public class DictionaryDescriptor<TDict, TKey, TValue> : CollectionDescriptor, ICollectionDescriptorWithContext where TDict : IDictionary<TKey, TValue>, new()
    {
        public override Type MappedType => typeof( TDict );

        public IContextSelector ElementSelector { get; set; }


        private IDescriptor _cachedKvpDescriptor;

        public override object CreateInitialTarget( SerializedData data, SerializationContext ctx )
        {
            return new TDict();
        }

        public override object Resize( object target, int newSize )
        {
            // When populating a dictionary, we must clear it first to ensure we populate from scratch.
            // This aligns with the "Replace" philosophy for population.
            ((TDict)target).Clear();
            return target;
        }

        public override int GetStepCount( object target )
        {
            return ((TDict)target).Count;
        }

        private IDescriptor GetKvpDescriptor( int stepIndex, object target )
        {
            if( _cachedKvpDescriptor != null )
                return _cachedKvpDescriptor;

            ContextSelectionArgs argsKey, argsVal;
            if( ElementSelector is UniformSelector uniformKey )
            {
                // Uniform selector selects based on the modulo of its internal array, and generic context parameters are handled by index,
                //   so instead of the step index, we put in 0 and 1 respectively.
                argsKey = new ContextSelectionArgs( 0, null, typeof( KeyValuePair<TKey, TValue> ), null, null, ((TDict)target).Count );
                argsVal = new ContextSelectionArgs( 1, null, typeof( KeyValuePair<TKey, TValue> ), null, null, ((TDict)target).Count );
                _cachedKvpDescriptor = new KeyValuePairDescriptor( uniformKey.Select( argsKey ), uniformKey.Select( argsVal ) );
                return _cachedKvpDescriptor;
            }

            argsKey = new ContextSelectionArgs( stepIndex, "key", typeof( KeyValuePair<TKey, TValue> ), null, null, ((TDict)target).Count );
            argsVal = new ContextSelectionArgs( stepIndex, "value", typeof( KeyValuePair<TKey, TValue> ), null, null, ((TDict)target).Count );
            return new KeyValuePairDescriptor( ElementSelector.Select( argsKey ), ElementSelector.Select( argsVal ) );
        }

        public override IEnumerator<IMemberInfo> GetMemberEnumerator( object target )
        {
            var dict = (TDict)target;
            int index = 0;
            foreach( var kvp in dict )
            {
                yield return new DictionaryEntryMemberInfo( index, kvp, GetKvpDescriptor( index, target ), true );
                index++;
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

            return new DictionaryEntryMemberInfo( stepIndex, kvp, GetKvpDescriptor( stepIndex, target ), isExisting );
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
                if( stepIndex == 0 ) 
                    return new KVPBufferMemberInfo( 0, "key", typeof( TKey ), _keyDescriptor );
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