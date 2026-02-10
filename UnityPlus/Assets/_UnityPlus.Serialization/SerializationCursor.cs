namespace UnityPlus.Serialization
{
    /// <summary>
    /// Represents the state of a single object currently being processed on the stack.
    /// </summary>
    public struct SerializationCursor
    {
        /// <summary>
        /// The current phase of this object's deserialization.
        /// </summary>
        public SerializationCursorPhase Phase;

        /// <summary>
        /// The object, parent, and access info encapsulated in a single struct.
        /// </summary>
        public TrackedObject TargetObj;

        /// <summary>
        /// Storage for constructor arguments during the Construction Phase.
        /// </summary>
        public object[] ConstructionBuffer;

        /// <summary>
        /// The descriptor that defines how to process this Target.
        /// </summary>
        public IDescriptor Descriptor;

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

        /// <summary>
        /// If true, the Target will be written back to the Parent via the Member accessor when this cursor is popped.
        /// Required for Deserialization (Assigning results) and Value Types (Propagating mutations).
        /// </summary>
        public bool WriteBackOnPop;
    }
}