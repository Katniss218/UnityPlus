using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.Networking.Types;

namespace UnityPlus.Serialization
{
    /// <summary>
    /// Maps the source type to a SerializedData directly, using methods.
    /// </summary>
    /// <typeparam name="TSource">The type of the object being mapped.</typeparam>
    public class DirectMapping<TSource> : SerializationMapping
    {
        public Func<TSource, IReverseReferenceMap, SerializedData> AsSerialized { get; set; }
        public Func<SerializedData, IForwardReferenceMap, TSource> AsObject { get; set; }

        public DirectMapping()
        {

        }

        public override SerializedData Save( object obj, IReverseReferenceMap s )
        {
            return AsSerialized.Invoke( (TSource)obj, s );
        }

        public override object Load( SerializedData data, IForwardReferenceMap l )
        {
            return AsObject.Invoke( data, l );
        }

        public override void LoadReferences( ref object obj, SerializedData data, IForwardReferenceMap l )
        {
            // Do nothing ...

#warning TODO - Add a direct reference mapping DirectReferenceMapping
        }
    }
}