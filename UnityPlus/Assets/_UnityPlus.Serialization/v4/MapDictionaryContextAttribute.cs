
using System;

namespace UnityPlus.Serialization
{
    /// <summary>
    /// Maps a context used on a dictionary to the contexts that should be used for its Keys and Values.
    /// </summary>
    [AttributeUsage( AttributeTargets.Assembly, AllowMultiple = true )]
    public sealed class MapDictionaryContextAttribute : Attribute
    {
        public int ContainerContext { get; }
        public int KeyContext { get; }
        public int ValueContext { get; }

        public MapDictionaryContextAttribute( int containerContext, int keyContext, int valueContext )
        {
            ContainerContext = containerContext;
            KeyContext = keyContext;
            ValueContext = valueContext;
        }

        public MapDictionaryContextAttribute( Type containerContext, Type keyContext, Type valueContext )
        {
            ContainerContext = ContextRegistry.GetId( containerContext );
            KeyContext = ContextRegistry.GetId( keyContext );
            ValueContext = ContextRegistry.GetId( valueContext );
        }
    }
}
