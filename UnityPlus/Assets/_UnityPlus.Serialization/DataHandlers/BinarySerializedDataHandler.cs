namespace UnityPlus.Serialization.DataHandlers
{
    /// <summary>
    /// Specialized handler for Binary files.
    /// </summary>
    public class BinarySerializedDataHandler : FileSerializedDataHandler
    {
        public BinarySerializedDataHandler( string filepath ) : base( filepath, BinaryFormat.Instance )
        {
        }
    }
}