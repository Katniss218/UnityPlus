using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnityPlus.Serialization.Patching.DSL.SyntaxTree
{
    /// <summary>
    /// An expression that returns a true or false when evaluated.
    /// </summary>
    public class BinaryExpression : IExpression
    {
#warning TODO - proper expression with precedence and arithmetic, etc.
        /*
        public IdentifierAccess Left { get; set; }
        public SerializedData LeftLiteral { get; set; }
        public IdentifierAccess Right { get; set; }
        public SerializedData RightLiteral { get; set; }
        */

        public IExpression Left { get; set; }
        public IExpression Right { get; set; }

        public IBinaryOperation Op { get; set; }

        public SerializedData Evaluate( TrackedSerializedData pivotItem )
        {
            /*
            SerializedData left = Left == null
                ? LeftLiteral
                : Left.GetFrom( pivotItem ).First().value;

            SerializedData right = Right == null
                ? RightLiteral
                : Right.GetFrom( pivotItem ).First().value;
            */

            SerializedData left = Left.GetFrom( pivotItem ).First().value;
            SerializedData right = Right.GetFrom( pivotItem ).First().value;

            SerializedData result = Op.Evaluate( left, right );
            return result;
        }
    }
}