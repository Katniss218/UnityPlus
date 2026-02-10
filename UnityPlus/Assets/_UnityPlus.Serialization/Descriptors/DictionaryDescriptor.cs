
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
        public override Type MappedType => typeof( TDict );

        public int KeyContext { get; set; } = ObjectContext.Default;
        public int ValueContext { get; set; } = ObjectContext.Default;

        // Cache the descriptor for entries to avoid allocation per item.
        private IDescriptor _cachedKvpDescriptor;

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

        private IDescriptor GetKvpDescriptor()
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
            public readonly string Name => null;
            public readonly int Index => _index; // The entry index in the serialized array
            public readonly Type MemberType => typeof( KeyValuePair<TKey, TValue> );
            public readonly bool RequiresWriteBack => true; // KVP is a struct
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
                // We return the value if it exists. 
                // The Driver and Descriptor determine whether to Populate or Construct.
                // Since KeyValuePairDescriptor now enforces Construction, returning the existing KVP 
                // will effectively be ignored for population purposes during Deserialization, 
                // but is vital for Serialization.
                return _isExisting ? (object)_kvp : null;
            }

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
            public override Type MappedType => typeof( KeyValuePair<TKey, TValue> );
            private readonly int _keyCtx;
            private readonly int _valCtx;

            // Context-specific descriptors are cached here so we don't look them up every GetMemberInfo
            private IDescriptor _keyDescriptor;
            private IDescriptor _valDescriptor;

            public KeyValuePairDescriptor( int keyCtx, int valCtx )
            {
                _keyCtx = keyCtx;
                _valCtx = valCtx;
                _keyDescriptor = TypeDescriptorRegistry.GetDescriptor( typeof( TKey ), _keyCtx );
                _valDescriptor = TypeDescriptorRegistry.GetDescriptor( typeof( TValue ), _valCtx );
            }

            // Always 2 steps.
            public override int GetStepCount( object target ) => 2;

            // CRITICAL FIX: KeyValuePair is immutable. It MUST use the Construction path (CreateInitialTarget -> Construct).
            // We must return > 0 here even if 'target' is a valid instance (Serialization), 
            // otherwise DeserializerStrategy will attempt 'Population' phase on an immutable struct, which fails.
            public override int GetConstructionStepCount( object target ) => 2;

            public override IMemberInfo GetMemberInfo( int stepIndex, object target )
            {
                // Both Serialization (target is KVP) and Deserialization (target is Buffer) use indices 0 and 1.
                if( stepIndex == 0 ) return new KVPBufferMemberInfo( 0, "key", typeof( TKey ), _keyDescriptor );
                return new KVPBufferMemberInfo( 1, "value", typeof( TValue ), _valDescriptor );
            }

            public override object CreateInitialTarget( SerializedData data, SerializationContext ctx )
            {
                // Buffer to hold Key and Value.
                return new object[2];
            }

            public override object Construct( object initialTarget )
            {
                var buf = (object[])initialTarget;
                // Unbox/Cast elements.
                TKey k = buf[0] != null ? (TKey)buf[0] : default;
                TValue v = buf[1] != null ? (TValue)buf[1] : default;
                return new KeyValuePair<TKey, TValue>( k, v );
            }

            private struct KVPBufferMemberInfo : IMemberInfo
            {
                public string Name { get; }
                public readonly int Index => -1; // Fields "key" and "value" are named
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
                    // Cannot write to KeyValuePair instance (immutable properties)
                }
            }
        }
    }
}
