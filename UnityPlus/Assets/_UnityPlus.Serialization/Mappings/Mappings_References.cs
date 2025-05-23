﻿
namespace UnityPlus.Serialization.Mappings
{
    public static class Mappings_References
    {
        [MapsAnyInterface( Context = ObjectContext.Ref )]
        [MapsAnyClass( Context = ObjectContext.Ref )]
        public static SerializationMapping ObjectRefMapping<T>() where T : class
        {
            return new PrimitiveSerializationMapping<T>( skipHeader: ObjectHeaderSkipMode.Always )
            {
                OnSave = ( o, s ) => s.RefMap.WriteObjectReference<T>( o ),
                OnLoad = ( data, l ) =>
                {
                    if( l.TryReadObjectReference<T>( data, out var obj ) )
                        return obj;
                    throw new UPSMissingReferenceException( $"missing reference, try again" );
                }
            };
        }
    }
}