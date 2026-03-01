using System;
using System.Collections.Generic;
using System.Linq;

namespace UnityPlus.Serialization
{
    public class DictionaryDescriptor<TDict, TKey, TValue> : CollectionDescriptor, ICollectionDescriptorWithContext where TDict : IDictionary<TKey, TValue>, new()
    {
        public override Type MappedType => typeof( TDict );
        public IContextSelector ElementSelector { get; set; }

        public override object CreateInitialTarget( SerializedData data, SerializationContext ctx )
        {
            return new TDict();
        }

        public override object Resize( object target, int newSize )
        {
            ((TDict)target).Clear();
            return target;
        }

        public override int GetStepCount( object target )
        {
            return ((TDict)target).Count;
        }

#warning TODO - finish.
        private IDescriptor _cachedKvpDescriptor;
        public override IMemberInfo GetMemberInfo( int stepIndex )
        {
            // Random Access Mode (Thin)
            return new DictionaryEntryMemberInfo( stepIndex, default, false, this );
        }

        public override IEnumerator<IMemberInfo> GetMemberEnumerator( object target )
        {
            var dict = (TDict)target;
            int index = 0;
            foreach( var kvp in dict )
            {
                // Enumeration Mode (Fat)
                yield return new DictionaryEntryMemberInfo( index, kvp, true, this );
                index++;
            }
        }

        private readonly struct DictionaryEntryMemberInfo : IMemberInfo
        {
            public string Name => null;
            public int Index => _index;
            public Type MemberType => typeof( KeyValuePair<TKey, TValue> );
            public bool RequiresWriteBack => true;

            // We let the strategy resolve the descriptor using GetContext
            public IDescriptor TypeDescriptor => null;

            private readonly int _index;
            private readonly KeyValuePair<TKey, TValue> _kvp; // Only used during enumeration
            private readonly bool _isExisting;
            private readonly DictionaryDescriptor<TDict, TKey, TValue> _parentDescriptor;

            public DictionaryEntryMemberInfo( int index, KeyValuePair<TKey, TValue> kvp, bool isExisting, DictionaryDescriptor<TDict, TKey, TValue> parent )
            {
                _index = index;
                _kvp = kvp;
                _isExisting = isExisting;
                _parentDescriptor = parent;
            }

            public ContextKey GetContext( object target )
            {
                // We need to construct a context key that represents KeyValuePair<KeyCtx, ValCtx>
                // We use the generic context mechanism.

                var dict = (TDict)target;

                var args1 = new ContextSelectionArgs( 0, typeof( KeyValuePair<TKey, TValue> ), typeof( KeyValuePair<TKey, TValue> ), dict.Count );
                var args2 = new ContextSelectionArgs( 1, typeof( KeyValuePair<TKey, TValue> ), typeof( KeyValuePair<TKey, TValue> ), dict.Count );

                ContextKey keyCtx = _parentDescriptor.ElementSelector.Select( args1 );
                ContextKey valCtx = _parentDescriptor.ElementSelector.Select( args2 );

                if( keyCtx == ContextKey.Default && valCtx == ContextKey.Default )
                    return ContextKey.Default;

                return ContextRegistry.GetOrRegisterGenericContext( typeof( KeyValuePair<,> ), new[] { keyCtx, valCtx } );
            }

            public object GetValue( object target )
            {
                if( !_isExisting )
                {
                    // Random Access Mode: Find element
                    var dict = (TDict)target;
                    if( _index < dict.Count )
                        return (object)dict.ElementAt( _index );
                    return null;
                }

                // Enumeration Mode: Use cached value
                return (object)_kvp;
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
    }
}