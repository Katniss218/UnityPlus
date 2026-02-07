namespace UnityPlus.Serialization
{
    public enum DeserializeResult
    {
        Success,
        Deferred, // Missing dependency, try again later
        Failed    // Fatal error or data corruption
    }
}