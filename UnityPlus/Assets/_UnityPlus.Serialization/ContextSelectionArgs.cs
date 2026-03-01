using System;

namespace UnityPlus.Serialization
{
    /// <summary>
    /// Arguments passed to an IContextSelector to determine the context for a child element.
    /// </summary>
    public readonly struct ContextSelectionArgs
    {
        /// <summary>
        /// The index of the child element in the collection (or tuple).
        /// </summary>
        public readonly int Index { get; }

        /// <summary>
        /// The Key identifier for this element.
        /// <para>For Arrays/Lists: Same as Index.</para>
        /// <para>For Objects: The Member Name (string).</para>
        /// <para>For Dictionaries: The Dictionary Key object (often string or int).</para>
        /// </summary>
        public readonly object Key { get; }

        /// <summary>
        /// The declared type of the storage location (e.g., T in List<T>).
        /// This is the type assumed if no polymorphic header is present.
        /// </summary>
        public readonly Type DeclaredType { get; }

        /// <summary>
        /// The actual type of the object instance.
        /// <para>On Serialization: The type of the live instance (`instance.GetType()`).</para>
        /// <para>On Deserialization: The resolved type from the `$type` header. Null if no header exists.</para>
        /// </summary>
        public readonly Type ActualType { get; }

        /// <summary>
        /// The serialized data for the child (Only available during Deserialization).
        /// </summary>
        public readonly SerializedData Data { get; }

        /// <summary>
        /// The total number of elements in the parent container. 
        /// (-1 if not applicable).
        /// </summary>
        public readonly int ContainerCount { get; }

        public ContextSelectionArgs( int index, object key, Type declaredType, Type actualType, SerializedData data, int containerCount )
        {
            Index = index;
            Key = key;
            DeclaredType = declaredType;
            ActualType = actualType ?? declaredType;
            Data = data;
            ContainerCount = containerCount;
        }
    }
}