using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnityPlus.Serialization.Patching.DSL.SyntaxTree
{
    public sealed class EqualOp : IBooleanOperation
    {
        public bool Evaluate( SerializedData left, SerializedData right )
        {
            return left.Equals( right );
        }
    }
}