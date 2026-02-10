namespace UnityPlus.Serialization.DataHandlers
{
    /// <summary>
    /// specialized handler for JSON files.
    /// </summary>
    public class JsonSerializedDataHandler : FileSerializedDataHandler
    {
        public JsonSerializedDataHandler( string filepath ) : base( filepath, JsonFormat.Instance )
        {
        }
    }
}
