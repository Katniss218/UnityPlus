using System;
using UnityEngine;

namespace UnityPlus.Serialization
{
    public static class ObjectFactory
    {
        private static readonly TypeMap<Func<Type, object>> _cache = new();

        public static object Create( SerializedObject data, IForwardReferenceMap l )
        {
            // this just instantiates.
            // call object.SetObjects( ... ) to deserialize the object parts.

            Type type = data[KeyNames.TYPE].ToType();
            Guid id = data[KeyNames.ID].ToGuid();

            object obj = null;
            if( _cache.TryGetClosest( type, out var factoryFunc ) )
            {
                obj = factoryFunc.Invoke( type );
            }
            else
            {
                obj = Activator.CreateInstance( type );
            }

            l.SetObj( id, obj );

            return obj;
        }
    }
}