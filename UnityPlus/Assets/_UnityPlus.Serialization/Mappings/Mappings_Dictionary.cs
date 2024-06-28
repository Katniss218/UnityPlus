using System;
using System.Collections.Generic;
using UnityEngine;

namespace UnityPlus.Serialization.Mappings
{
    public static class Mappings_Dictionary
    {
        [SerializationMappingProvider( typeof( KeyValuePair<,> ), Context = KeyValueContext.ValueToValue )]
        public static SerializationMapping KeyValuePair_ValueToValue_Mapping<TKey, TValue>()
        {
            return new NonPrimitiveSerializationMapping<KeyValuePair<TKey, TValue>>()
            {
                OnSave = ( o, s ) =>
                {
                    var mapping = SerializationMappingRegistry.GetMapping<TKey>( ObjectContext.Default, o.Key );
                    var keyData = MappingHelper.DoSave<TKey>( mapping, o.Key, s );

                    mapping = SerializationMappingRegistry.GetMapping<TValue>( ObjectContext.Default, o.Value );
                    var valueData = MappingHelper.DoSave<TValue>( mapping, o.Value, s );

                    SerializedObject kvpData = new SerializedObject()
                    {
                        { "key", keyData },
                        { "value", valueData }
                    };

                    return kvpData;
                },
                OnInstantiate = ( data, l ) =>
                {
                    return data == null ? default : new KeyValuePair<TKey, TValue>();
                },
                OnLoad = ( ref KeyValuePair<TKey, TValue> o, SerializedData data, ILoader l ) =>
                {
                    SerializedData keyData = data["key"];
                    SerializedData valueData = data["value"];

                    TKey key = default;
                    var mapping = MappingHelper.GetMapping_Load<TKey>( ObjectContext.Default, MappingHelper.GetTypeOf<TKey>( keyData ), keyData, l );
                    MappingHelper.DoLoad( mapping, ref key, keyData, l );

                    TValue value = default;
                    mapping = MappingHelper.GetMapping_Load<TValue>( ObjectContext.Default, MappingHelper.GetTypeOf<TValue>( valueData ), valueData, l );
                    MappingHelper.DoLoad( mapping, ref value, valueData, l );

                    o = new KeyValuePair<TKey, TValue>( key, value );
                },
                OnLoadReferences = ( ref KeyValuePair<TKey, TValue> o, SerializedData data, ILoader l ) =>
                {
                    SerializedData keyData = data["key"];
                    SerializedData valueData = data["value"];

                    TKey key = o.Key;
                    var mapping = MappingHelper.GetMapping_LoadReferences<TKey>( ObjectContext.Default, key, keyData, l );
                    MappingHelper.DoLoadReferences( mapping, ref key, keyData, l );

                    TValue value = o.Value;
                    mapping = MappingHelper.GetMapping_LoadReferences<TValue>( ObjectContext.Default, value, valueData, l );
                    MappingHelper.DoLoadReferences( mapping, ref value, valueData, l );

                    o = new KeyValuePair<TKey, TValue>( key, value );
                }
            };
        }

        [SerializationMappingProvider( typeof( Dictionary<,> ), Context = KeyValueContext.ValueToValue )]
        public static SerializationMapping Dictionary_ValueToValue_Mapping<TKey, TValue>()
        {
            return new NonPrimitiveSerializationMappingWithTemp<(TKey, TValue)[], Dictionary<TKey, TValue>>()
            {
                OnSave = ( o, s ) =>
                {
                    if( o == null )
                        return null;

#warning TODO - add some way of automatically adding type and id. (also, objects should be objects not arrays)
                    SerializedArray arr = new SerializedArray();

                    foreach( var kvp in o )
                    {
                        var mapping = SerializationMappingRegistry.GetMapping<TKey>( ObjectContext.Default, kvp.Key );
                        var keyData = MappingHelper.DoSave<TKey>( mapping, kvp.Key, s );

                        mapping = SerializationMappingRegistry.GetMapping<TValue>( ObjectContext.Default, kvp.Value );
                        var valueData = MappingHelper.DoSave<TValue>( mapping, kvp.Value, s );

                        SerializedObject kvpData = new SerializedObject()
                        {
                            { "key", keyData },
                            { "value", valueData }
                        };

                        arr.Add( kvpData );
                    }

                    return arr;
                },
                OnInstantiate = ( data, l ) =>
                {
                    return data == null ? default : new Dictionary<TKey, TValue>();
                },
                OnInstantiateTemp = ( data, l ) =>
                {
                    if( data is not SerializedArray dataObj )
                        return null;

                    return data == null ? null : new (TKey, TValue)[dataObj.Count];
                },
                OnLoad = ( NonPrimitiveSerializationMappingWithTemp<(TKey, TValue)[], Dictionary<TKey, TValue>> self, ref Dictionary<TKey, TValue> o, SerializedData data, ILoader l ) =>
                {
                    if( data is not SerializedArray dataObj )
                        return;

                    int i = 0;
                    foreach( var dataKvp in dataObj )
                    {
                        SerializedData keyData = dataKvp["key"];
                        SerializedData valueData = dataKvp["value"];

                        TKey key = default;
                        var mapping = MappingHelper.GetMapping_Load<TKey>( ObjectContext.Default, MappingHelper.GetTypeOf<TKey>( keyData ), keyData, l );
                        MappingHelper.DoLoad( mapping, ref key, keyData, l );

                        TValue value = default;
                        mapping = MappingHelper.GetMapping_Load<TValue>( ObjectContext.Default, MappingHelper.GetTypeOf<TValue>( valueData ), valueData, l );
                        MappingHelper.DoLoad( mapping, ref value, valueData, l );

                        self.temp[i] = (key, value);
                        i++;
                    }
                },
                OnLoadReferences = ( NonPrimitiveSerializationMappingWithTemp<(TKey, TValue)[], Dictionary<TKey, TValue>> self, ref Dictionary<TKey, TValue> o, SerializedData data, ILoader l ) =>
                {
                    if( data is not SerializedArray dataObj )
                        return;

                    if( self.temp == null )
                        return;

                    int i = 0;
                    foreach( var (key, value) in self.temp )
                    {
                        SerializedData dataKvp = dataObj[i];

                        SerializedData keyData = dataKvp["key"];
                        SerializedData valueData = dataKvp["value"];

                        TKey key2 = key;
                        var mapping = MappingHelper.GetMapping_LoadReferences<TKey>( ObjectContext.Default, key2, keyData, l );
                        MappingHelper.DoLoadReferences( mapping, ref key2, keyData, l );

                        TValue value2 = value;
                        mapping = MappingHelper.GetMapping_LoadReferences<TValue>( ObjectContext.Default, value2, valueData, l );
                        MappingHelper.DoLoadReferences( mapping, ref value2, valueData, l );

                        if( key2 != null )
                            o[key2] = value2;

                        i++;
                    }
                }
            };
        }

        [SerializationMappingProvider( typeof( Dictionary<,> ), Context = KeyValueContext.RefToValue )]
        public static SerializationMapping Dictionary_TKey_TValue_Mapping<TKey, TValue>()
        {
            return new NonPrimitiveSerializationMappingWithTemp<(TKey, TValue)[], Dictionary<TKey, TValue>>()
            {
                OnSave = ( o, s ) =>
                {
                    if( o == null )
                        return null;

#warning TODO - add some way of automatically adding type and id. (also, objects should be objects not arrays)
                    SerializedArray arr = new SerializedArray();

                    foreach( var kvp in o )
                    {
                        var mapping = SerializationMappingRegistry.GetMapping<TKey>( ObjectContext.Ref, kvp.Key );
                        var keyData = MappingHelper.DoSave<TKey>( mapping, kvp.Key, s );

                        mapping = SerializationMappingRegistry.GetMapping<TValue>( ObjectContext.Default, kvp.Value );
                        var valueData = MappingHelper.DoSave<TValue>( mapping, kvp.Value, s );

                        SerializedObject kvpData = new SerializedObject()
                        {
                            { "key", keyData },
                            { "value", valueData }
                        };

                        arr.Add( kvpData );
                    }

                    return arr;
                },
                OnInstantiate = ( data, l ) =>
                {
                    return data == null ? default : new Dictionary<TKey, TValue>();
                },
                OnInstantiateTemp = ( data, l ) =>
                {
                    if( data is not SerializedArray dataObj )
                        return null;

                    return data == null ? null : new (TKey, TValue)[dataObj.Count];
                },
                OnLoad = ( NonPrimitiveSerializationMappingWithTemp<(TKey, TValue)[], Dictionary<TKey, TValue>> self, ref Dictionary<TKey, TValue> o, SerializedData data, ILoader l ) =>
                {
                    if( data is not SerializedArray dataObj )
                        return;

                    int i = 0;
                    foreach( var dataKvp in dataObj )
                    {
                        SerializedData keyData = dataKvp["key"];
                        SerializedData valueData = dataKvp["value"];

                        TKey key = default;
                        var mapping = MappingHelper.GetMapping_Load<TKey>( ObjectContext.Ref, MappingHelper.GetTypeOf<TKey>( keyData ), keyData, l );
                        MappingHelper.DoLoad( mapping, ref key, keyData, l );

                        TValue value = default;
                        mapping = MappingHelper.GetMapping_Load<TValue>( ObjectContext.Default, MappingHelper.GetTypeOf<TValue>( valueData ), valueData, l );
                        MappingHelper.DoLoad( mapping, ref value, valueData, l );

                        self.temp[i] = (key, value);
                        i++;
                    }
                },
                OnLoadReferences = ( NonPrimitiveSerializationMappingWithTemp<(TKey, TValue)[], Dictionary<TKey, TValue>> self, ref Dictionary<TKey, TValue> o, SerializedData data, ILoader l ) =>
                {
                    if( data is not SerializedArray dataObj )
                        return;

                    if( self.temp == null )
                        return;

                    int i = 0;
                    foreach( var (key, value) in self.temp )
                    {
                        SerializedData dataKvp = dataObj[i];

                        SerializedData keyData = dataKvp["key"];
                        SerializedData valueData = dataKvp["value"];

                        TKey key2 = key;
                        var mapping = MappingHelper.GetMapping_LoadReferences<TKey>( ObjectContext.Ref, key2, keyData, l );
                        MappingHelper.DoLoadReferences( mapping, ref key2, keyData, l );

                        TValue value2 = value;
                        mapping = MappingHelper.GetMapping_LoadReferences<TValue>( ObjectContext.Default, value2, valueData, l );
                        MappingHelper.DoLoadReferences( mapping, ref value2, valueData, l );

                        if( key2 != null )
                            o[key2] = value2;

                        i++;
                    }
                }
            };
        }
    }
}