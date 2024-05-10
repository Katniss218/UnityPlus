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
    public class DirectMapping<TSource> : SerializationMapping
    {
        public Func<TSource, IReverseReferenceMap, SerializedData> AsSerialized { get; set; }
        public Func<SerializedData, IForwardReferenceMap, TSource> AsObject { get; set; }
        // maps to an object directly.
    }
}