using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnityPlus.Serialization.Patching.DSL.SyntaxTree
{
    public interface IBinaryOperation
    {
        SerializedData Evaluate( SerializedData left, SerializedData right );
    }
}