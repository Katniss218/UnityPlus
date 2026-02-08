
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
            bool isExisting = stepIndex < dict.Count;
            if( isExisting )
            {
                // O(N) access - Dictionaries are not indexable. 
                kvp = dict.ElementAt( stepIndex );
            }

            return new DictionaryEntryMemberInfo( stepIndex, kvp, GetKvpDescriptor(), isExisting );
        }

        private struct DictionaryEntryMemberInfo : IMemberInfo
        {
            public string Name => null;
            public int Index => _index; // The entry index in the serialized array
            public Type MemberType => typeof( KeyValuePair<TKey, TValue> );
            public bool IsValueType => true; // KVP is a struct
            public ITypeDescriptor TypeDescriptor => _descriptor;

            private int _index;
            private KeyValuePair<TKey, TValue> _kvp;
            private ITypeDescriptor _descriptor;
            private bool _isExisting;

            public DictionaryEntryMemberInfo( int index, KeyValuePair<TKey, TValue> kvp, ITypeDescriptor descriptor, bool isExisting )
            {
                _index = index;
                _kvp = kvp;
                _descriptor = descriptor;
                _isExisting = isExisting;
            }

            // Target is the dictionary.
            // If this represents an existing entry, return it.
            // If this is a new entry (append), return null to force the Deserializer to Construct it.
            // Returning default(KVP) would cause the Deserializer to skip Construction (treating it as PopulateExisting).
            public object GetValue( object target ) => _isExisting ? (object)_kvp : null;

            public void SetValue( ref object target, object value )
            {
                // This is called when the StackMachine finishes processing the KVP.
                // We add the result back to the Dictionary.
                var dict = (TDict)target;
                var pair = (KeyValuePair<TKey, TValue>)value;

                // Safety check: Key cannot be null in a dictionary
                if( pair.Key == null )
                    return;

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

            // Use Construction Phase pattern to allow fully constructing the KVP from an object array buffer
            public override int GetConstructionStepCount( object target ) => 2;

            public override IMemberInfo GetMemberInfo( int stepIndex, object target )
            {
                if( stepIndex == 0 ) return new KVPBufferMemberInfo( 0, "key", typeof( TKey ), _keyDescriptor );
                return new KVPBufferMemberInfo( 1, "value", typeof( TValue ), _valDescriptor );
            }

            public override object CreateInitialTarget( SerializedData data, SerializationContext ctx )
            {
                // Buffer to hold Key and Value. StackMachineDriver creates this automatically if ConstructionStepCount > 0
                // but we can return it here explicitly if needed.
                return new object[2];
            }

            public override object Construct( object initialTarget )
            {
                var buf = (object[])initialTarget;
                // Unbox/Cast elements.
                // Handle potential nulls if construction failed partially
                TKey k = buf[0] != null ? (TKey)buf[0] : default;
                TValue v = buf[1] != null ? (TValue)buf[1] : default;
                return new KeyValuePair<TKey, TValue>( k, v );
            }

            private struct KVPBufferMemberInfo : IMemberInfo
            {
                public string Name { get; }
                public int Index => -1; // Fields "key" and "value" are named
                public Type MemberType { get; }
                public bool IsValueType => MemberType.IsValueType;
                public ITypeDescriptor TypeDescriptor { get; }

                private int _index;

                public KVPBufferMemberInfo( int index, string name, Type type, ITypeDescriptor desc )
                {
                    _index = index;
                    Name = name;
                    MemberType = type;
                    TypeDescriptor = desc;
                }

                public object GetValue( object target )
                {
                    // Serialize: target is KeyValuePair
                    if( target is KeyValuePair<TKey, TValue> pair )
                        return _index == 0 ? (object)pair.Key : pair.Value;

                    // Deserialize: target is object[] buffer
                    if( target is object[] buf )
                        return buf[_index];

                    return null;
                }

                public void SetValue( ref object target, object value )
                {
                    // Deserialize: Write to buffer
                    if( target is object[] buf )
                    {
                        buf[_index] = value;
                    }
                }
            }
        }
    }
}
