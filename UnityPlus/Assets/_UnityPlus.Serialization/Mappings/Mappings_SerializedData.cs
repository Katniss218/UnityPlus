using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityPlus.Serialization;

namespace UnityPlus.Serialization.Mappings
{
    public static class Mappings_SerializedData
    {
        [SerializationMappingProvider( typeof( SerializedPrimitive ) )]
        public static SerializationMapping SerializedPrimitiveMapping()
        {
#warning TODO - this should be wrapped in a serializedobject with id, type, and value (and same as any other primitive in typed context).
            return new PrimitiveObjectSerializationMapping<SerializedPrimitive>()
            {
                OnSave = ( o, s ) => o,
                OnInstantiate = ( data, l ) => data as SerializedPrimitive
            };
        }

        [SerializationMappingProvider( typeof( SerializedObject ) )]
        public static SerializationMapping SerializedObjectMapping()
        {
            return new PrimitiveObjectSerializationMapping<SerializedObject>()
            {
                OnSave = ( o, s ) => o,
                OnInstantiate = ( data, l ) => data as SerializedObject
            };
        }

        [SerializationMappingProvider( typeof( SerializedArray ) )]
        public static SerializationMapping SerializedArrayMapping()
        {
            return new PrimitiveObjectSerializationMapping<SerializedArray>()
            {
                OnSave = ( o, s ) => o,
                OnInstantiate = ( data, l ) => data as SerializedArray
            };
        }
    }
}