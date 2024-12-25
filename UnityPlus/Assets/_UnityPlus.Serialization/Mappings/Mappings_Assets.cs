using System;

namespace UnityPlus.Serialization.Mappings
{
    public static class Mappings_Assets
    {
        [MapsInheritingFrom( typeof( object ), Context = ObjectContext.Asset )]
        public static SerializationMapping ObjectAssetMapping<T>() where T : class
        {
            return new PrimitiveSerializationMapping<T>()
            {
                OnSave = ( o, s ) => s.RefMap.WriteAssetReference<T>( o ),
                OnInstantiate = ( data, l ) => l.ReadAssetReference<T>( data )
            };
        }

        [MapsInheritingFrom( typeof( Array ), Context = ArrayContext.Assets )]
        public static SerializationMapping ArrayAssetMapping<T>() where T : class
        {
            return new IndexedSerializationMapping<T[], T>( o => o.Length,
                ObjectContext.Asset,
                ( o, i ) => // writes to data
                {
                    return o[i];
                },
                ( o, i, oElem ) => // loads from data
                {
                    o[i] = oElem;
                } )
                .WithFactory( length => new T[length] );
            /*
            return new PrimitiveStructSerializationMapping<T[]>()
            {
                OnSave = ( o, s ) =>
                {
                    SerializedArray serializedArray = new SerializedArray();
                    for( int i = 0; i < o.Length; i++ )
                    {
                        var data = s.RefMap.WriteAssetReference<T>( o[i] );

                        serializedArray.Add( data );
                    }

                    return serializedArray;
                },
                OnInstantiate = ( data, l ) =>
                {
                    SerializedArray serializedArray = (SerializedArray)data;

                    T[] array = new T[serializedArray.Count];

                    for( int i = 0; i < serializedArray.Count; i++ )
                    {
                        SerializedData elementData = serializedArray[i];

                        var element = l.ReadAssetReference<T>( elementData );
                        array[i] = element;
                    }

                    return array;
                }
            };*/
        }
    }
}