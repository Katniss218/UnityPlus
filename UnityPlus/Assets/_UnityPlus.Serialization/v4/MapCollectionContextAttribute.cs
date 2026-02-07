
using System;

namespace UnityPlus.Serialization
{
    /// <summary>
    /// Maps a context used on a collection (Array/List) to the context that should be used for its elements.
    /// </summary>
    [AttributeUsage( AttributeTargets.Assembly, AllowMultiple = true )]
    public sealed class MapCollectionContextAttribute : Attribute
    {
        public int ContainerContext { get; }
        public int ElementContext { get; }

        public MapCollectionContextAttribute( int containerContext, int elementContext )
        {
            ContainerContext = containerContext;
            ElementContext = elementContext;
        }

        public MapCollectionContextAttribute( Type containerContext, Type elementContext )
        {
            ContainerContext = ContextRegistry.GetId( containerContext );
            ElementContext = ContextRegistry.GetId( elementContext );
        }
    }
}
