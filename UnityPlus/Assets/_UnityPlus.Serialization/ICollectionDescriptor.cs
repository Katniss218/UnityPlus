namespace UnityPlus.Serialization
{
    /// <summary>
    /// Describes a resizable collection (Array, List).
    /// </summary>
    public interface ICollectionDescriptor : ICompositeDescriptor
    {
        object Resize( object target, int newSize );
    }
}