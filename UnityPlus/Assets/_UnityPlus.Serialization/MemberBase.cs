
namespace UnityPlus.Serialization
{
    /// <summary>
    /// Saves and loads part of an object of type <typeparamref name="TSource"/>.
    /// </summary>
    /// <typeparam name="TSource">The type that this item belongs to.</typeparam>
    public abstract class MemberBase<TSource>
    {
        /// <summary>
        /// Serializes the member as a child of <paramref name="sourceData"/>
        /// </summary>
        /// <returns>
        /// True if the member has been fully serialized, false if the method needs to be called again to serialize more.
        /// </returns>
        public abstract MappingResult Save( TSource source, SerializedData sourceData, ISaver s );

        /// <summary>
        /// Instantiates the member from a child of <paramref name="sourceData"/>.
        /// </summary>
        /// <remarks>
        /// Does NOT assign the member to the <paramref name="source"/> instance. Use <see cref="Assign"/> for that.
        /// </remarks>
        /// <returns>
        /// True if the member has been fully deserialized, false if the method needs to be called again to deserialize more.
        /// </returns>
        public abstract MappingResult Load( ref object member, SerializedData sourceData, ILoader l );

        public abstract void Assign( ref TSource source, object member );

        public abstract MemberBase<TSource> Copy();
    }
}