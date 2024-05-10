using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnityPlus.Serialization
{
    /// <summary>
    /// Factory mapping.
    /// </summary>
    /// <typeparam name="TSource">The type of the object being mapped.</typeparam>
    public class DirectMapping<TSource> : SerializationMapping, IObjectMapping<TSource>, IDataMapping<TSource>
    {
        public Func<TSource, IReverseReferenceMap, SerializedData> AsSerialized { get; set; }
        public Func<SerializedData, IForwardReferenceMap, TSource> AsObject { get; set; }

        public SerializedObject GetObjectsPass( TSource source, IReverseReferenceMap s )
        {
            throw new NotImplementedException( "replace serializedobject with serializeddata for general case" );
        }

        public void SetObjectsPass( TSource source, SerializedObject data, IForwardReferenceMap l )
        {
            throw new NotImplementedException( "replace serializedobject with serializeddata for general case" );
        }

        public SerializedData GetDataPass( TSource source, IReverseReferenceMap s )
        {
            return AsSerialized.Invoke( source, s );
        }

        public void SetDataPass( TSource source, SerializedData data, IForwardReferenceMap l )
        {
            AsObject.Invoke( data, l );
        }

        // maps to an object directly.
    }
}