
namespace UnityPlus.Serialization
{
    /// <summary>
    /// The default mapping, returned when no mapping was found.
    /// </summary>
    public class EmptySerializationMapping<TSource> : SerializationMapping
    {
        public override SerializedData Save( object obj, IReverseReferenceMap s )
        {
            return null;
        }

        public override object Load( SerializedData data, IForwardReferenceMap l )
        {
            return default( TSource );
        }

        public override void LoadReferences( object obj, SerializedData data, IForwardReferenceMap l )
        {
            // Do nothing.
        }
    }
}