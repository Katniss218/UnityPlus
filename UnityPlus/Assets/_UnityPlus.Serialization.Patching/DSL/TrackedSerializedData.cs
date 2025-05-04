
namespace UnityPlus.Serialization.Patching.DSL
{
    /// <summary>
    /// A struct that keeps track of the parent of the current item, to allow the current item to be changed in the hierarchy.
    /// </summary>
    public struct TrackedSerializedData
    {
        // kind of a hack since I'd need to change the data inside the parent object array via changing a reference, but the reference can't be stored here easily.
        public readonly SerializedData value;
        public readonly SerializedData parent;
        public readonly string name;
        public readonly int index;

        public bool IsByIndex => index != -1;
        public bool IsByName => index == -1;

        public TrackedSerializedData( SerializedData value )
        {
            this.value = value;
            this.parent = null;
            this.name = null;
            this.index = -1;
        }
        
        public TrackedSerializedData( SerializedData value, SerializedData parent, string name )
        {
            this.value = value;
            this.parent = parent;
            this.name = name;
            this.index = -1;
        }

        public TrackedSerializedData( SerializedData value, SerializedData parent, int index )
        {
            this.value = value;
            this.parent = parent;
            this.name = null;
            this.index = index;
        }
    }
}