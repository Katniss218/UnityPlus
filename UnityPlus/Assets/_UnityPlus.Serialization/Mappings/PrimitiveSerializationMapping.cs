using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnityPlus.Serialization
{
    public delegate void LoadReferencesAction<TSource>( ref TSource obj, SerializedData data, IForwardReferenceMap l );

    /// <summary>
    /// Maps the source type to a SerializedData directly, using methods.
    /// </summary>
    /// <typeparam name="TSource">The type of the object being mapped.</typeparam>
    public class PrimitiveSerializationMapping<TSource> : SerializationMapping
    {
        /// <summary>
        /// The function invoked to convert the C# object into its serialized representation.
        /// </summary>
        public Func<TSource, IReverseReferenceMap, SerializedData> OnSave { get; set; }

        /// <summary>
        /// The function invoked to convert the serialized representation back into its corresponding C# object.
        /// </summary>
        public Func<SerializedData, IForwardReferenceMap, TSource> OnLoad { get; set; }

#warning TODO - primitives require to be pre-populsted on creation, so they will only ever have one step.
        /// <summary>
        /// The function invoked to fill in the references in the created object.
        /// </summary>
        public LoadReferencesAction<TSource> OnPopulate { get; set; }

        public PrimitiveSerializationMapping()
        {

        }

        public override SerializedData Save( object obj, IReverseReferenceMap s )
        {
            return OnSave.Invoke( (TSource)obj, s );
        }

        public override object Load( SerializedData data, IForwardReferenceMap l )
        {
            if( OnLoad != null )
                return OnLoad.Invoke( data, l );
            return default( TSource );
        }

        public override void Populate( ref object obj, SerializedData data, IForwardReferenceMap l )
        {
            if( OnPopulate != null )
            {
                // obj may be null here.
                var obj2 = (TSource)obj;
                OnPopulate.Invoke( ref obj2, data, l );
                obj = obj2;
            }
            // Do nothing ...
        }
    }
}