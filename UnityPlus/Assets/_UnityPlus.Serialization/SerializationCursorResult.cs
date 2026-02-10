namespace UnityPlus.Serialization
{
    public enum SerializationCursorResult
    {
        /// <summary>
        /// Keep processing the current cursor
        /// </summary>
        Continue,
        /// <summary>
        /// Pop the current cursor
        /// </summary>
        Finished,
        /// <summary>
        /// Exit the tick loop (time budget)
        /// </summary>
        Paused,
        /// <summary>
        /// A new cursor was pushed; loop should restart to process top
        /// </summary>
        PushedDependency,
        /// <summary>
        /// The current cursor cannot proceed due to missing deps; pop it (it's queued)
        /// </summary>
        Deferred
    }
}