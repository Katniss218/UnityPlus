using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace UnityPlus.Serialization.Mappings
{
    public static class Mappings_Dictionary
    {
#warning TODO - transform the 'dict' into an array of key-value pairs.

        [SerializationMappingProvider( typeof( Dictionary<,> ), Context = KeyValueContext.ValueToValue )]
        public static SerializationMapping Dictionary_ValueToValue_Mapping<TKey, TValue>()
        {
            return new NonPrimitiveSerializationMapping<Dictionary<TKey, TValue>>()
            {
                OnSave = ( o, s ) =>
                {
                    var mapping = SerializationMappingRegistry.GetMappingOrEmpty( ObjectContext.Default, typeof( KeyValuePair<TKey, TValue>[] ) );

                    return mapping.Save( o.ToArray(), s );
                },
                OnInstantiate = ( data, l ) =>
                {
                    return new Dictionary<TKey, TValue>();
                },
                OnLoad = ( ref Dictionary<TKey, TValue> o, SerializedData data, IForwardReferenceMap l ) =>
                {
                    if( data is not SerializedArray dataObj )
                        return;

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

                        var mapping = SerializationMappingRegistry.GetMappingOrDefault<TKey>( ObjectContext.Default, keyType );

                        object key;
                        switch( mapping.SerializationStyle )
                        {
                            default:
                                continue;
                            case SerializationStyle.PrimitiveStruct:
                                key = mapping.Instantiate( keyData, l );
                                break;
                            case SerializationStyle.NonPrimitive:
                                key = mapping.Instantiate( keyData, l );
                                mapping.Load( ref key, keyData, l );
                                break;
                        }

                        mapping = SerializationMappingRegistry.GetMappingOrDefault<TValue>( ObjectContext.Default, valueType );

                        object value;
                        switch( mapping.SerializationStyle )
                        {
                            default:
                                continue;
                            case SerializationStyle.PrimitiveStruct:
                                value = mapping.Instantiate( valueData, l );
                                break;
                            case SerializationStyle.NonPrimitive:
                                value = mapping.Instantiate( valueData, l );
                                mapping.Load( ref value, valueData, l );
                                break;
                        }

                        o[(TKey)key] = (TValue)value;
                    }
                },
                OnLoadReferences = ( ref Dictionary<TKey, TValue> o, SerializedData data, IForwardReferenceMap l ) =>
                {
                    if( data is not SerializedArray dataObj )
                        return;

                    int current = 0;
                    foreach( var (key, value) in o )
#warning TODO - if key / value is struct - instantiate. otherwise retrieve from the refMap and set internal fields.
                    {
                        SerializedData dataKvp = dataObj[current];

                        SerializedData keyData = dataKvp["key"];
                        SerializedData valueData = dataKvp["value"];

                        Type elementType = typeof( TValue );
                        if( dataKvp.TryGetValue( KeyNames.TYPE, out var elementType2 ) )
                        {
                            elementType = elementType2.DeserializeType();
                        }

                        var mapping = SerializationMappingRegistry.GetMappingOrDefault<TKey>( ObjectContext.Default, key );

#warning TODO - compress this switch into something that will handle it.
                        object key2 = key;
                        switch( mapping.SerializationStyle )
                        {
                            default:
                                continue;
                            case SerializationStyle.PrimitiveObject:
                                key2 = mapping.Instantiate( keyData, l );
                                break;
                            case SerializationStyle.NonPrimitive:
                                mapping.LoadReferences( ref key2, keyData, l );
                                break;
                        }

                        mapping = SerializationMappingRegistry.GetMappingOrDefault<TValue>( ObjectContext.Default, value );

                        object value2 = value;
                        switch( mapping.SerializationStyle )
                        {
                            default:
                                continue;
                            case SerializationStyle.PrimitiveObject:
                                value2 = mapping.Instantiate( valueData, l );
                                break;
                            case SerializationStyle.NonPrimitive:
                                mapping.LoadReferences( ref value2, valueData, l );
                                break;
                        }

#warning TODO - this is forbidden. This should serialize an array of key-value pairs instead of a dict directly.
                        o[(TKey)key2] = (TValue)value2;
                        current++;
                    }
                }
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
                OnLoad = ( ref KeyValuePair<TKey, TValue> o, SerializedData data, IForwardReferenceMap l ) =>
                {
                    SerializedData keyData = data["key"];
                    SerializedData valueData = data["value"];

                    Type keyType = keyData.TryGetValue( KeyNames.TYPE, out var elementType2 )
                        ? elementType2.DeserializeType()
                        : typeof( TKey );

                    Type valueType = valueData.TryGetValue( KeyNames.TYPE, out elementType2 )
                        ? elementType2.DeserializeType()
                        : typeof( TKey );

                    var mapping = SerializationMappingRegistry.GetMappingOrDefault<TKey>( ObjectContext.Default, keyType );

                    object key;
                    switch( mapping.SerializationStyle )
                    {
                        default:
                            return;
                        case SerializationStyle.PrimitiveStruct:
                            key = mapping.Instantiate( keyData, l );
                            break;
                        case SerializationStyle.NonPrimitive:
                            key = mapping.Instantiate( keyData, l );
                            mapping.Load( ref key, keyData, l );
                            break;
                    }

                    mapping = SerializationMappingRegistry.GetMappingOrDefault<TValue>( ObjectContext.Default, valueType );

                    object value;
                    switch( mapping.SerializationStyle )
                    {
                        default:
                            return;
                        case SerializationStyle.PrimitiveStruct:
                            value = mapping.Instantiate( valueData, l );
                            break;
                        case SerializationStyle.NonPrimitive:
                            value = mapping.Instantiate( valueData, l );
                            mapping.Load( ref value, valueData, l );
                            break;
                    }
                },
                OnLoadReferences = ( ref KeyValuePair<TKey, TValue> o, SerializedData data, IForwardReferenceMap l ) =>
                {
                    SerializedData keyData = data["key"];
                    SerializedData valueData = data["value"];

                    var mapping = SerializationMappingRegistry.GetMappingOrDefault<TKey>( ObjectContext.Default, o.Key );

#warning TODO - compress this switch into something that will handle it.
                    object key = o.Key;
                    switch( mapping.SerializationStyle )
                    {
                        default:
                            return;
                        case SerializationStyle.PrimitiveObject:
                            key = mapping.Instantiate( keyData, l );
                            break;
                        case SerializationStyle.NonPrimitive:
                            mapping.LoadReferences( ref key, keyData, l );
                            break;
                    }

                    mapping = SerializationMappingRegistry.GetMappingOrDefault<TValue>( ObjectContext.Default, o.Value );

                    object value = o.Value;
                    switch( mapping.SerializationStyle )
                    {
                        default:
                            return;
                        case SerializationStyle.PrimitiveObject:
                            value = mapping.Instantiate( valueData, l );
                            break;
                        case SerializationStyle.NonPrimitive:
                            mapping.LoadReferences( ref value, valueData, l );
                            break;
                    }

                    o = new KeyValuePair<TKey, TValue>( (TKey)key, (TValue)value );
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

                        string keyName = s.GetID( key ).SerializeGuidAsKey();
                        obj[keyName] = data;
                    }

                    return obj;
                },
                OnInstantiate = ( data, l ) =>
                {
                    return new Dictionary<TKey, TValue>();
                },
                OnLoad = null,
                OnLoadReferences = ( ref Dictionary<TKey, TValue> o, SerializedData data, IForwardReferenceMap l ) =>
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

                        TKey keyObj = (TKey)l.GetObj( key.DeserializeGuidAsKey() );

                        o[keyObj] = (TValue)elem;
                    }
                }
            };
        }
    }
}
