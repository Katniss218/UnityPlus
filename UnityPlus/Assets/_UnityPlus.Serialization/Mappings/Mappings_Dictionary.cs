﻿using System;
using System.Collections.Generic;
using UnityEngine;

namespace UnityPlus.Serialization.Mappings
{
    public static class Mappings_Dictionary
    {
        
        [SerializationMappingProvider( typeof( ValueTuple<,> ) )]
        public static SerializationMapping ValueTupleMapping<T1, T2>()
        {
            return new MemberwiseSerializationMapping<(T1, T2)>()
            {
                ("1", new Member<(T1, T2), T1>( o => o.Item1 )),
                ("2", new Member<(T1, T2), T2>( o => o.Item2 ))
            };
        }


        [SerializationMappingProvider( typeof( KeyValuePair<,> ), Context = KeyValueContext.ValueToValue )]
        public static SerializationMapping KeyValuePair_ValueToValue_Mapping<TKey, TValue>()
        {
            return new NonPrimitiveSerializationMapping<KeyValuePair<TKey, TValue>>()
            {
                OnSave = ( o, s ) =>
                {
                    var mapping = SerializationMappingRegistry.GetMappingOrDefault<TKey>( ObjectContext.Default, o.Key );
                    var keyData = mapping.Save( o.Key, s );

                    mapping = SerializationMappingRegistry.GetMappingOrDefault<TValue>( ObjectContext.Default, o.Value );
                    var valueData = mapping.Save( o.Value, s );

                    SerializedObject kvpData = new SerializedObject()
                    {
                        { "key", keyData },
                        { "value", valueData }
                    };

                    return kvpData;
                },
                OnInstantiate = ( data, l ) =>
                {
                    return new KeyValuePair<TKey, TValue>();
                },
                OnLoad = ( ref KeyValuePair<TKey, TValue> o, SerializedData data, ILoader l ) =>
                {
                    SerializedData keyData = data["key"];
                    SerializedData valueData = data["value"];

                    Type keyType = keyData.TryGetValue( KeyNames.TYPE, out var elementType2 )
                        ? elementType2.DeserializeType()
                        : typeof( TKey );

                    Type valueType = valueData.TryGetValue( KeyNames.TYPE, out elementType2 )
                        ? elementType2.DeserializeType()
                        : typeof( TKey );

                    TKey key = default;
                    var mapping = SerializationMappingRegistry.GetMappingOrDefault<TKey>( ObjectContext.Default, keyType );
                    MappingHelper.DoLoad( mapping, ref key, keyData, l );

                    TValue value = default;
                    mapping = SerializationMappingRegistry.GetMappingOrDefault<TValue>( ObjectContext.Default, valueType );
                    MappingHelper.DoLoad( mapping, ref value, valueData, l );

                    o = new KeyValuePair<TKey, TValue>( key, value );
                },
                OnLoadReferences = ( ref KeyValuePair<TKey, TValue> o, SerializedData data, ILoader l ) =>
                {
                    SerializedData keyData = data["key"];
                    SerializedData valueData = data["value"];

#warning TODO - if generics are classes, load from refmap?
                    TKey key = o.Key;
                    var mapping = SerializationMappingRegistry.GetMappingOrDefault<TKey>( ObjectContext.Default, key );
                    MappingHelper.DoLoadReferences( mapping, ref key, keyData, l );

                    TValue value = o.Value;
                    mapping = SerializationMappingRegistry.GetMappingOrDefault<TValue>( ObjectContext.Default, value );
                    MappingHelper.DoLoadReferences( mapping, ref value, valueData, l );

                    o = new KeyValuePair<TKey, TValue>( key, value );
                }
            };
        }

        [SerializationMappingProvider( typeof( Dictionary<,> ), Context = KeyValueContext.ValueToValue )]
        public static SerializationMapping Dictionary_ValueToValue_Mapping<TKey, TValue>()
        {
            return new NonPrimitiveSerializationMapping2<(TKey, TValue)[], Dictionary<TKey, TValue>>()
            {
                OnSave = ( o, s ) =>
                {
#warning TODO - add some way of automatically adding type and id. (also, objects should be objects not arrays)
                    SerializedArray arr = new SerializedArray();

                    foreach( var kvp in o )
                    {
                        var mapping = SerializationMappingRegistry.GetMappingOrDefault<TKey>( ObjectContext.Default, kvp.Key );
                        var keyData = mapping.Save( kvp.Key, s );

                        mapping = SerializationMappingRegistry.GetMappingOrDefault<TValue>( ObjectContext.Default, kvp.Value );
                        var valueData = mapping.Save( kvp.Value, s );

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
                    return new Dictionary<TKey, TValue>();
                },
                OnInstantiateTemp = ( data, l ) =>
                {
                    if( data is not SerializedArray dataObj )
                        return null;

                    return new (TKey, TValue)[dataObj.Count];
                },
                OnLoad = ( NonPrimitiveSerializationMapping2<(TKey, TValue)[], Dictionary<TKey, TValue>> self, ref Dictionary<TKey, TValue> o, SerializedData data, ILoader l ) =>
                {
                    if( data is not SerializedArray dataObj )
                        return;

                    int i = 0;
                    foreach( var dataKvp in dataObj )
                    {
                        SerializedData keyData = dataKvp["key"];
                        SerializedData valueData = dataKvp["value"];

                        Type keyType = keyData.TryGetValue( KeyNames.TYPE, out var elementType2 )
                            ? elementType2.DeserializeType()
                            : typeof( TKey );

                        Type valueType = valueData.TryGetValue( KeyNames.TYPE, out elementType2 )
                            ? elementType2.DeserializeType()
                            : typeof( TKey );

                        TKey key = default;
                        var mapping = SerializationMappingRegistry.GetMappingOrDefault<TKey>( ObjectContext.Default, keyType );
                        MappingHelper.DoLoad( mapping, ref key, keyData, l );

                        TValue value = default;
                        mapping = SerializationMappingRegistry.GetMappingOrDefault<TValue>( ObjectContext.Default, valueType );
                        MappingHelper.DoLoad( mapping, ref value, valueData, l );

                        if( key == null )
                            continue;

                        self.temp[i] = (key, value);
                        i++;
                    }
                },
                OnLoadReferences = ( NonPrimitiveSerializationMapping2<(TKey, TValue)[], Dictionary<TKey, TValue>> self, ref Dictionary<TKey, TValue> o, SerializedData data, ILoader l ) =>
                {
                    if( data is not SerializedArray dataObj )
                        return;

                    int i = 0;
                    foreach( var (key, value) in self.temp )
                    {
                        SerializedData dataKvp = dataObj[i];

                        SerializedData keyData = dataKvp["key"];
                        SerializedData valueData = dataKvp["value"];

                        Type elementType = typeof( TValue );
                        if( dataKvp.TryGetValue( KeyNames.TYPE, out var elementType2 ) )
                        {
                            elementType = elementType2.DeserializeType();
                        }

                        TKey key2 = key;
                        var mapping = SerializationMappingRegistry.GetMappingOrDefault<TKey>( ObjectContext.Default, key2 );
                        MappingHelper.DoLoadReferences( mapping, ref key2, keyData, l );

                        TValue value2 = value;
                        mapping = SerializationMappingRegistry.GetMappingOrDefault<TValue>( ObjectContext.Default, value2 );
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
            return new NonPrimitiveSerializationMapping<Dictionary<TKey, TValue>>()
            {
                OnSave = ( o, s ) =>
                {
                    SerializedObject obj = new SerializedObject();

                    foreach( var (key, value) in o )
                    {
                        var mapping = SerializationMappingRegistry.GetMappingOrDefault<TValue>( ObjectContext.Default, value );

                        var data = mapping.Save( value, s );

                        string keyName = s.RefMap.GetID( key ).SerializeGuidAsKey();
                        obj[keyName] = data;
                    }

                    return obj;
                },
                OnInstantiate = ( data, l ) =>
                {
                    return new Dictionary<TKey, TValue>();
                },
                OnLoad = null,
                OnLoadReferences = ( ref Dictionary<TKey, TValue> o, SerializedData data, ILoader l ) =>
                {
                    if( data is not SerializedObject dataObj )
                        return;

                    foreach( var (key, value) in dataObj )
                    {
                        SerializedData elementData = value;

                        Type elementType = typeof( TValue );
                        if( elementData.TryGetValue( KeyNames.TYPE, out var elementType2 ) )
                        {
                            elementType = elementType2.DeserializeType();
                        }

                        var mapping = SerializationMappingRegistry.GetMappingOrDefault<TValue>( ObjectContext.Default, elementType );

#warning TODO - fix this mess.
                        // Calling `mapping.Load` inside LoadReferences makes the objects inside the dict unable to be referenced by other external objects.
                        object elem = mapping.Instantiate( elementData, l );

                        mapping.Load( ref elem, elementData, l );
                        mapping.LoadReferences( ref elem, elementData, l );

                        TKey keyObj = (TKey)l.RefMap.GetObj( key.DeserializeGuidAsKey() );

                        o[keyObj] = (TValue)elem;
                    }
                }
            };
        }
    }
}