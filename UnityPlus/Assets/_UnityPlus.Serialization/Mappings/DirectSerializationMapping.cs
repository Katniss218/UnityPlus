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
    public class DirectSerializationMapping<TSource> : SerializationMapping
    {
        /// <summary>
        /// The function invoked to convert the C# object into its serialized representation.
        /// </summary>
        public Func<TSource, IReverseReferenceMap, SerializedData> SaveFunc { get; set; }

        /// <summary>
        /// The function invoked to convert the serialized representation back into its corresponding C# object.
        /// </summary>
        public Func<SerializedData, IForwardReferenceMap, TSource> LoadFunc { get; set; }

        public DirectSerializationMapping()
        {

        }

        public override SerializedData Save( object obj, IReverseReferenceMap s )
        {
            return SaveFunc.Invoke( (TSource)obj, s );
        }

        public override object Load( SerializedData data, IForwardReferenceMap l )
        {
            return LoadFunc.Invoke( data, l );
        }

        public override void LoadReferences( object obj, SerializedData data, IForwardReferenceMap l )
        {
            // Do nothing ...
        }
    }
}