using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnityPlus.Serialization.Patching.DSL.SyntaxTree
{
    public abstract class Statement : IStatementOrTransformation
    {
        public abstract void Invoke( IEnumerable<TrackedSerializedData> pivot );
    }
}