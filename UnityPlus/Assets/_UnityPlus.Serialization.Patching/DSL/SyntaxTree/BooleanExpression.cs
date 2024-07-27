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
    public class BooleanExpression : IExpression<bool>
    {
#warning TODO - proper expression with precedence and arithmetic, etc.

        public IdentifierAccess Left { get; set; }
        public SerializedData LeftLiteral { get; set; }
        public IdentifierAccess Right { get; set; }
        public SerializedData RightLiteral { get; set; }

        public IBooleanOperation Op { get; set; }

        public bool Evaluate( TrackedSerializedData item )
        {
            SerializedData left = Left == null
                ? LeftLiteral
                : Left.GetFrom( item ).First().value;
            
            SerializedData right = Right == null
                ? RightLiteral
                : Right.GetFrom( item ).First().value;

            bool result = Op.Evaluate( left, right );
            return result;
        }
    }
}