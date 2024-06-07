using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnityPlus.Serialization
{
    /// <summary>
    /// Maps an object that can contain references to other objects.
    /// </summary>
    /// <typeparam name="TSource">The type of the object being mapped.</typeparam>
    public class PrimitiveObjectSerializationMapping<TSource> : SerializationMapping
    {
        /// <summary>
        /// The function invoked to convert the C# object into its serialized representation.
        /// </summary>
        public Func<TSource, IReverseReferenceMap, SerializedData> OnSave { get; set; }

        /// <summary>
        /// The function invoked to convert the serialized representation back into its corresponding C# object.
        /// </summary>
        public Func<SerializedData, IForwardReferenceMap, TSource> OnInstantiate { get; set; }

        public override SerializationStyle SerializationStyle => SerializationStyle.PrimitiveObject;

        public PrimitiveObjectSerializationMapping()
        {

        }

        public override SerializedData Save( object obj, IReverseReferenceMap s )
        {
            return OnSave.Invoke( (TSource)obj, s );
        }

        public override object Instantiate( SerializedData data, IForwardReferenceMap l )
        {
            if( OnInstantiate != null )
                return OnInstantiate.Invoke( data, l );
            return default( TSource );
        }

        public override void Load( ref object obj, SerializedData data, IForwardReferenceMap l )
        {
            throw new InvalidOperationException( $"Load is not supported on `{nameof( PrimitiveObjectSerializationMapping<TSource> )}`." );
        }

        public override void LoadReferences( ref object obj, SerializedData data, IForwardReferenceMap l )
        {
            throw new InvalidOperationException( $"LoadReferences is not supported on `{nameof( PrimitiveObjectSerializationMapping<TSource> )}`." );
        }
    }
}