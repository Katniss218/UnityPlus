
namespace UnityPlus.Serialization
{
    public abstract class SerializationMapping
    {
        public abstract SerializedData Save( object obj, IReverseReferenceMap s );
        public abstract object Load( SerializedData data, IForwardReferenceMap l );
        public abstract void LoadReferences( ref object obj, SerializedData data, IForwardReferenceMap l );
    }
}