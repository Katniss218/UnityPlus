namespace UnityPlus.Serialization
{
    public interface ICollectionDescriptorWithContext : ICollectionDescriptor
    {
        ContextKey ElementContext { get; set; }
    }

    /// <summary>
    /// Describes a resizable collection (Array, List).
    /// </summary>
    public interface ICollectionDescriptor : ICompositeDescriptor
    {
        object Resize( object target, int newSize );
    }
}