using System;
using System.Collections.Generic;

namespace UnityPlus.Serialization.Mappings
{
    public static class Mappings_Dictionary
    {
        [MapsInheritingFrom( typeof( KeyValuePair<,> ), Context = KeyValueContext.ValueToValue )]
        public static SerializationMapping KeyValuePair_ValueToValue_Mapping<TKey, TValue>()
        {
            return new MemberwiseSerializationMapping<KeyValuePair<TKey, TValue>>()
                .WithMember( "key", o => o.Key, ( ref KeyValuePair<TKey, TValue> o, TKey value ) => { } )
                .WithMember( "value", o => o.Value, ( ref KeyValuePair<TKey, TValue> o, TValue value ) => { } )
                .WithFactory<TKey, TValue>( ( key, value ) => new KeyValuePair<TKey, TValue>( key, value ) );
        }
        
        [MapsInheritingFrom( typeof( KeyValuePair<,> ), Context = KeyValueContext.RefToValue )]
        public static SerializationMapping KeyValuePair_RefToValue_Mapping<TKey, TValue>()
        {
            return new MemberwiseSerializationMapping<KeyValuePair<TKey, TValue>>()
                .WithMember( "key", ObjectContext.Ref, o => o.Key, ( ref KeyValuePair<TKey, TValue> o, TKey value ) => { } )
                .WithMember( "value", o => o.Value, ( ref KeyValuePair<TKey, TValue> o, TValue value ) => { } )
                .WithFactory<TKey, TValue>( ( key, value ) => new KeyValuePair<TKey, TValue>( key, value ) );
        }

        [MapsInheritingFrom( typeof( Dictionary<,> ), Context = KeyValueContext.ValueToValue )]
        public static SerializationMapping Dictionary_ValueToValue_Mapping<TKey, TValue>()
        {
            return new EnumeratedSerializationMapping<Dictionary<TKey, TValue>, KeyValuePair<TKey, TValue>>(
                KeyValueContext.ValueToValue,
                ( o, i, oElem ) =>
                {
                    o[oElem.Key] = oElem.Value;
                } )
                .WithFactory( ( int length ) => new Dictionary<TKey, TValue>( length ) );
        }

        [MapsInheritingFrom( typeof( Dictionary<,> ), Context = KeyValueContext.RefToValue )]
        public static SerializationMapping Dictionary_TKey_TValue_Mapping<TKey, TValue>()
        {
            return new EnumeratedSerializationMapping<Dictionary<TKey, TValue>, KeyValuePair<TKey, TValue>>(
                KeyValueContext.RefToValue,
                ( o, i, oElem ) =>
                {
                    o[oElem.Key] = oElem.Value;
                } )
                .WithFactory( ( int length ) => new Dictionary<TKey, TValue>( length ) );
        }
    }
}