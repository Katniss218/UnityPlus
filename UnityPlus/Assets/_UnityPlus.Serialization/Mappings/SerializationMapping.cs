
using System;

namespace UnityPlus.Serialization
{
    public abstract class SerializationMapping
    {
        // The reason these are `object` instead of being generically typed is that when getting the mappings for `Component`
        //   `SerializationMapping<Transform>` can't be cast to `SerializationMapping<Component>`

        /// <summary>
        /// Saves the state of the object <paramref name="obj"/>.
        /// </summary>
        public abstract SerializedData Save( object obj, IReverseReferenceMap s );
        /// <summary>
        /// Loads (creates) the object from <paramref name="data"/>.
        /// </summary>
        public abstract object Load( SerializedData data, IForwardReferenceMap l );
        /// <summary>
        /// Populates the members of the object <paramref name="obj"/>, if applicable.
        /// </summary>
        public abstract void Populate( ref object obj, SerializedData data, IForwardReferenceMap l );

        public static SerializationMapping Empty( Type sourceType )
        {
            return (SerializationMapping)Activator.CreateInstance( typeof( EmptySerializationMapping<> ).MakeGenericType( sourceType ) );
        }

        public static SerializationMapping Empty<TSource>()
        {
            return new EmptySerializationMapping<TSource>();
        }
    }
}