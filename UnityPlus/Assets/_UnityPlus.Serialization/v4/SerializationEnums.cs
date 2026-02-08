
namespace UnityPlus.Serialization
{
    public enum StepResult
    {
        Continue,           // Keep processing the current cursor
        Finished,           // Pop the current cursor
        Paused,             // Exit the tick loop (time budget)
        PushedDependency,   // A new cursor was pushed; loop should restart to process top
        Deferred            // The current cursor cannot proceed due to missing deps; pop it (it's queued)
    }

    public enum ResolveResult
    {
        Resolved,
        RequiresPush,
        Deferred,
        Failed
    }
}
