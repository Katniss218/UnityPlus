
namespace UnityPlus.Serialization
{
    public enum CursorPhase
    {
        /// <summary>
        /// Gathering arguments for the constructor. Target is null.
        /// </summary>
        Construction,

        /// <summary>
        /// Setting members on the instantiated object. Target is valid.
        /// </summary>
        Population
    }

    /// <summary>
    /// Represents the state of a single object currently being processed on the stack.
    /// </summary>
    public struct SerializationCursor
    {
        /// <summary>
        /// The current phase of this object's deserialization.
        /// </summary>
        public CursorPhase Phase;

        /// <summary>
        /// The actual object instance being serialized/deserialized.
        /// If this is a struct, 'Target' holds the Boxed value.
        /// During Construction Phase, this is null.
        /// </summary>
        public object Target;

        /// <summary>
        /// Storage for constructor arguments during the Construction Phase.
        /// </summary>
        public object[] ConstructionBuffer;

        /// <summary>
        /// The descriptor that defines how to process this Target.
        /// </summary>
        public ITypeDescriptor Descriptor;

        /// <summary>
        /// The current step (arg index or member index) we are processing within the current Phase.
        /// </summary>
        public int StepIndex;

        /// <summary>
        /// The number of steps in the Construction Phase.
        /// </summary>
        public int ConstructionStepCount;

        /// <summary>
        /// The number of steps in the Population Phase.
        /// </summary>
        public int PopulationStepCount;

        /// <summary>
        /// The SerializedData node associated with this object.
        /// </summary>
        public SerializedData DataNode;

        // --- Struct Write-Back Support ---

        /// <summary>
        /// If true, 'Target' is a boxed value type copy, and must be written back to 'ParentTarget' when this cursor is popped.
        /// </summary>
        public bool NeedsWriteBack;

        /// <summary>
        /// The object that owns this struct (if NeedsWriteBack is true).
        /// </summary>
        public object ParentTarget;

        /// <summary>
        /// The member info used to write the modified struct back to the parent.
        /// </summary>
        public IMemberInfo ParentMemberInfo;
    }
}