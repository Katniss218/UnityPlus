using System;

namespace UnityPlus.Serialization.Mappings
{
    public static class Mappings_References
    {
        [MapsAnyInterface( Context = ObjectContext.Ref )]
        [MapsAnyClass( Context = ObjectContext.Ref )]
        public static SerializationMapping ObjectRefMapping<T>() where T : class
        {
            return new PrimitiveSerializationMapping<T>()
            {
                OnSave = ( o, s ) => s.RefMap.WriteObjectReference<T>( o ),
                OnInstantiate = ( data, l ) => l.ReadObjectReference<T>( data )
            };
        }

        [MapsInheritingFrom( typeof( Array ), Context = ArrayContext.Refs )]
        public static SerializationMapping ArrayReferenceMapping<T>() where T : class
        {
            return new IndexedSerializationMapping<T[], T>( o => o.Length,
                ObjectContext.Ref,
                ( o, i ) => // writes to data
                {
                    return o[i];
                },
                ( o, i, oElem ) => // loads from data
                {
                    o[i] = oElem;
                } )
                .WithFactory( length => new T[length] );
        }
    }
}