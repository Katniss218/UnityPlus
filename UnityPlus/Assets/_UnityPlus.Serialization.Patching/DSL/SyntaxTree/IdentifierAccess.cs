using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnityPlus.Serialization.Patching.DSL.SyntaxTree
{
    public class IdentifierAccess
    {
        public SerializedDataPath Path { get; set; }

        public IEnumerable<TrackedSerializedData> GetFrom( TrackedSerializedData pivotItem )
        {
            return Path.Evaluate( pivotItem );
        }
    }
}