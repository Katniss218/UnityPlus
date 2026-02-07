
using System;
using System.Collections.Generic;
using System.Linq;

namespace UnityPlus.Serialization
{
    public interface IDictionaryDescriptor
    {
        int KeyContext { get; set; }
        int ValueContext { get; set; }
    }

    public class DictionaryDescriptor<TDict, TKey, TValue> : CollectionDescriptor, IDictionaryDescriptor
        where TDict : IDictionary<TKey, TValue>, new()
    {
        public override Type WrappedType => typeof( TDict );

        public int KeyContext { get; set; } = ObjectContext.Default;
        public int ValueContext { get; set; } = ObjectContext.Default;

        // Cache the descriptor for entries to avoid allocation per item.
        private ITypeDescriptor _cachedKvpDescriptor;

        // Dictionaries are serialized as an Array of KeyValuePairs (or struct equivalent)
        // [ {key: K, value: V}, {key: K, value: V} ]

        public override object CreateInitialTarget( SerializedData data, SerializationContext ctx )
        {
            // Note: Dictionaries don't support pre-sizing via constructor easily like Lists do, so we just return new.
            // The data unwrapping logic (Object -> Array) handles the cursor traversing the right node,
            // so we don't strictly need to inspect data here unless we wanted to optimize capacity.
            return new TDict();
        }

        public override object Resize( object target, int newSize )
        {
            // Dictionaries cannot be resized like arrays. 
            // We return the target as is. The StackMachine will add entries via GetMemberInfo steps.
            return target;
        }

        public override int GetStepCount( object target )
        {
            return ((TDict)target).Count;
        }

        private ITypeDescriptor GetKvpDescriptor()
        {
            if( _cachedKvpDescriptor == null )
            {
                _cachedKvpDescriptor = new KeyValuePairDescriptor( KeyContext, ValueContext );
            }
            return _cachedKvpDescriptor;
        }

        public override IMemberInfo GetMemberInfo( int stepIndex, object target )
        {
            var dict = (TDict)target;

            // Handle Append Mode (Deserialization where DataCount > DictCount)
            KeyValuePair<TKey, TValue> kvp = default;
            if( stepIndex < dict.Count )
            {
                // O(N) access - Dictionaries are not indexable. 
                kvp = dict.ElementAt( stepIndex );
            }

            return new DictionaryEntryMemberInfo( stepIndex, kvp, GetKvpDescriptor() );
        }

        private struct DictionaryEntryMemberInfo : IMemberInfo
        {
            public string Name => null; // Array element
            public Type MemberType => typeof( KeyValuePair<TKey, TValue> );
            public bool IsValueType => true; // KVP is a struct
            public ITypeDescriptor TypeDescriptor => _descriptor;

            private int _index;
            private KeyValuePair<TKey, TValue> _kvp;
            private ITypeDescriptor _descriptor;

            public DictionaryEntryMemberInfo( int index, KeyValuePair<TKey, TValue> kvp, ITypeDescriptor descriptor )
            {
                _index = index;
                _kvp = kvp;
                _descriptor = descriptor;
            }

            public object GetValue( object target ) => _kvp; // Target is the dictionary, but we return the KVP we captured

            public void SetValue( ref object target, object value )
            {
                // This is called when the StackMachine finishes processing the KVP.
                // We add the result back to the Dictionary.
                var dict = (TDict)target;
                var pair = (KeyValuePair<TKey, TValue>)value;

                // If keys match existing, update. If new, add.
                // Note: Iteration order stability in Dicts is not guaranteed if we modify while iterating.
                // But deserialization is usually additive.
                if( dict.ContainsKey( pair.Key ) )
                    dict[pair.Key] = pair.Value;
                else
                    dict.Add( pair.Key, pair.Value );
            }
        }

        private class KeyValuePairDescriptor : CompositeDescriptor
        {
            public override Type WrappedType => typeof( KeyValuePair<TKey, TValue> );
            private readonly int _keyCtx;
            private readonly int _valCtx;

            // Context-specific descriptors are cached here so we don't look them up every GetMemberInfo
            private ITypeDescriptor _keyDescriptor;
            private ITypeDescriptor _valDescriptor;

            public KeyValuePairDescriptor( int keyCtx, int valCtx )
            {
                _keyCtx = keyCtx;
                _valCtx = valCtx;
                _keyDescriptor = TypeDescriptorRegistry.GetDescriptor( typeof( TKey ), _keyCtx );
                _valDescriptor = TypeDescriptorRegistry.GetDescriptor( typeof( TValue ), _valCtx );
            }

            public override int GetStepCount( object target ) => 2;

            public override IMemberInfo GetMemberInfo( int stepIndex, object target )
            {
                if( stepIndex == 0 ) return new KVPMemberInfo( "key", typeof( TKey ), _keyDescriptor, true );
                return new KVPMemberInfo( "value", typeof( TValue ), _valDescriptor, false );
            }

            public override object CreateInitialTarget( SerializedData data, SerializationContext ctx )
            {
                // Immutable struct, but we need a mutable container for the stack machine to write Key and Value into
                // before we construct the final KVP.
                return new KVPBuffer();
            }

            public override object Construct( object initialTarget )
            {
                var buf = (KVPBuffer)initialTarget;
                return new KeyValuePair<TKey, TValue>( buf.Key, buf.Value );
            }

            private class KVPBuffer
            {
                public TKey Key;
                public TValue Value;
            }

            private struct KVPMemberInfo : IMemberInfo
            {
                public string Name { get; }
                public Type MemberType { get; }
                public bool IsValueType => MemberType.IsValueType;
                public ITypeDescriptor TypeDescriptor { get; }

                private bool _isKey;

                public KVPMemberInfo( string name, Type type, ITypeDescriptor desc, bool isKey )
                {
                    Name = name;
                    MemberType = type;
                    TypeDescriptor = desc;
                    _isKey = isKey;
                }

                public object GetValue( object target )
                {
                    if( target is KeyValuePair<TKey, TValue> pair )
                        return _isKey ? pair.Key : pair.Value;

                    if( target is KVPBuffer buf )
                        return _isKey ? buf.Key : buf.Value;

                    return null;
                }

                public void SetValue( ref object target, object value )
                {
                    if( target is KVPBuffer buf )
                    {
                        if( _isKey ) buf.Key = (TKey)value;
                        else buf.Value = (TValue)value;
                    }
                }
            }
        }
    }
}
