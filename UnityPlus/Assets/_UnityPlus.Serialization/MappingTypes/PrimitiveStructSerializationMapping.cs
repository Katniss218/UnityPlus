using System;

namespace UnityPlus.Serialization
{
    /// <summary>
    /// Maps an object that can contain references to other objects.
    /// </summary>
    /// <typeparam name="TSource">The type of the object being mapped.</typeparam>
    public sealed class PrimitiveStructSerializationMapping<TSource> : SerializationMapping
    {
        /// <summary>
        /// The function invoked to convert the C# object into its serialized representation.
        /// </summary>
        public Func<TSource, ISaver, SerializedData> OnSave { get; set; }

        /// <summary>
        /// The function invoked to convert the serialized representation back into its corresponding C# object.
        /// </summary>
        public Func<SerializedData, IForwardReferenceMap, TSource> OnInstantiate { get; set; }

        public PrimitiveStructSerializationMapping()
        {

        }


        protected override bool Save<T>( T obj, ref SerializedData data, ISaver s )
        {
            data = OnSave.Invoke( (TSource)(object)obj, s );
            return true;
        }

        protected override bool TryPopulate<T>( ref T obj, SerializedData data, ILoader l )
        {
            return false;
        }

        protected override bool TryLoad<T>( ref T obj, SerializedData data, ILoader l )
        {
            return false;
        }

        protected override bool TryLoadReferences<T>( ref T obj, SerializedData data, ILoader l )
        {
            if( OnInstantiate == null )
                return false;

            // Instantiating in LoadReferences means that every object that can be referenced should have already been added to the ILoader's RefMap.
            TSource obj2 = OnInstantiate.Invoke( data, l.RefMap );
            obj = (T)(object)obj2;

            return true;
        }
    }
}