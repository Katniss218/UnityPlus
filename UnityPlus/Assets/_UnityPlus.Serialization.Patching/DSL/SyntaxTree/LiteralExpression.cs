using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnityPlus.Serialization.Patching.DSL.SyntaxTree
{
    public class LiteralExpression : IExpression
    {
        public SerializedData Value { get; set; }

        public SerializedData Evaluate( TrackedSerializedData pivotItem )
        {
            return this.Value;
        }
    }
}