using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnityPlus.Serialization.Patching.DSL.SyntaxTree
{
    internal class LiteralExpression : IExpression
    {
        SerializedData LiteralValue { get; set; }

        public SerializedData Evaluate( TrackedSerializedData pivotItem )
        {
            return this.LiteralValue;
        }
    }
}