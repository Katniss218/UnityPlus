using System;

namespace UnityPlus.Serialization.Mappings
{
    public static class Mappings_Array
    {
        [MapsInheritingFrom( typeof( Array ), Context = ArrayContext.Values )]
        public static SerializationMapping ArrayMapping<T>()
        {
#warning TODO - multidimensional arrays?
            return new IndexedSerializationMapping<T[], T>( o => o.Length,
                ( o, i ) => // writes to data
                {
                    return o[i];
                },
                ( o, i, oElem ) => // loads from data
                {
                    o[i] = oElem;
                } )
                .WithFactory( (int length ) => new T[length] );
        }
    }
}