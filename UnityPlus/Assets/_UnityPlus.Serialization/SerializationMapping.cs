
using System;
using System.Reflection;

namespace UnityPlus.Serialization
{
    /// <summary>
    /// Defines how the mapping should be used.
    /// </summary>
    /// <remarks>
    /// See <see cref="MappingHelper"/>'s DoSave/Load/LoadReferences methods for usage.
    /// </remarks>
    public enum SerializationStyle
    {
        /// <summary>
        /// save --- instantiate (called inside load)
        /// </summary>
        PrimitiveStruct,

        /// <summary>
        /// save --- instantiate (called inside loadreferences)
        /// </summary>
        PrimitiveObject,

        /// <summary>
        /// save --- instantiate, load, loadreferences
        /// </summary>
        NonPrimitive
    }

    /// <summary>
    /// Represents an arbitrary serialization mapping.
    /// </summary>
    public abstract class SerializationMapping
    {
        /*
        
        SerializationMapping, by design, should be able to decide how to serialize/deserialize any value (including null).
        - This means that no shoft-circuiting is possible in the code using the mappings.

        */

        internal int context;
        /// <summary>
        /// Gets the serialization context in which this mapping operates.
        /// </summary>
        public int Context => this.context;

        /// <summary>
        /// Gets the serializatino style of this mapping type.
        /// </summary>
        public abstract SerializationStyle SerializationStyle { get; }

        /// <returns>Either itself, or a clone (depending on if the mapping needs to persist data between Load and LoadReferences).</returns>
        public virtual SerializationMapping GetInstance()
        {
            return this;
        }

        // Mappings use `object` instead of being generic - the reason for this is that `SerializationMapping<Transform>` can't be cast to `SerializationMapping<Component>`.
        // This could be PARTIALLY resolved using variant interfaces - variance is not supported on `ref` parameters.

        /// <summary>
        /// Saves the full state of the object.
        /// </summary>
        /// <param name="obj">The object being saved.</param>
        public abstract SerializedData Save( object obj, ISaver s );

        /// <summary>
        /// Creates an "empty" instance of the type being loaded.
        /// </summary>
        /// <returns>The object being loaded.</returns>
        public abstract object Instantiate( SerializedData data, ILoader l );

        /// <summary>
        /// Loads members that use a mapping with <see cref="SerializationStyle.PrimitiveStruct"/> serialization style. 
        /// </summary>
        /// <param name="obj">The object being loaded.</param>
        public abstract void Load( ref object obj, SerializedData data, ILoader l );

        /// <summary>
        /// Loads members that use a mapping with <see cref="SerializationStyle.PrimitiveObject"/> serialization style. 
        /// </summary>
        /// <param name="obj">The object being loaded.</param>
        public abstract void LoadReferences( ref object obj, SerializedData data, ILoader l );
    }
}