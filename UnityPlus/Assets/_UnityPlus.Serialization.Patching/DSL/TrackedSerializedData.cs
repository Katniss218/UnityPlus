using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnityPlus.Serialization.Patching.DSL
{
    public struct TrackedSerializedData
    {
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