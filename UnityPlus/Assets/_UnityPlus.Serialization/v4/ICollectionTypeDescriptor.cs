namespace UnityPlus.Serialization
{
    /// <summary>
    /// Describes a resizable collection (Array, List).
    /// </summary>
    public interface ICollectionTypeDescriptor : ICompositeTypeDescriptor
    {
        object Resize( object target, int newSize );
    }
}