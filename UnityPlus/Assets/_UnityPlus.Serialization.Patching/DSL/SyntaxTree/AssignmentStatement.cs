using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnityPlus.Serialization.Patching.DSL.SyntaxTree
{
    public class AssignmentStatement : Statement
    {
        public IdentifierAccess Left { get; set; }

        public BinaryExpression Right { get; set; }
        public SerializedData RightLiteral { get; set; }

        public override void Invoke( IEnumerable<TrackedSerializedData> pivot )
        {
            foreach( var pivotItem in pivot )
            {
                IEnumerable<TrackedSerializedData> newPivots = Left.GetFrom( pivotItem );
                foreach( var newPivotItem in newPivots )
                {
#warning TODO - a can a serialized data expression even return multiple things?
                    SerializedData rightItems = Right == null
                        ? RightLiteral
                        : Right.Evaluate( newPivotItem );

#warning TODO - Make a SerializedArray if there are many returned rightItems.
                    if( newPivotItem.IsByName )
                        newPivotItem.parent[newPivotItem.name] = rightItems;
                    else
                        newPivotItem.parent[newPivotItem.index] = rightItems;
                }
            }
        }
    }
}