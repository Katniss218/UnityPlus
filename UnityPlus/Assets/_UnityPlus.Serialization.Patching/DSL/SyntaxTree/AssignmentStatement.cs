using System.Collections.Generic;

namespace UnityPlus.Serialization.Patching.DSL.SyntaxTree
{
    /// <summary>
    /// An assignment statement in the form of `<left> = <right>`.
    /// </summary>
    public class AssignmentStatement : IStatement
    {
        /// <summary>
        /// The expression result will be assigned to all entries returned by this path.
        /// </summary>
        public SerializedDataPath Left { get; set; }

        /// <summary>
        /// The expression the result of which to assign to the entries.
        /// </summary>
        public IExpression Right { get; set; }

        public void Invoke( IEnumerable<TrackedSerializedData> pivot )
        {
            foreach( var pivotItem in pivot )
            {
                IEnumerable<TrackedSerializedData> newPivots = Left.Evaluate( pivotItem );
                foreach( var newPivotItem in newPivots )
                {
                    SerializedData rightItem = Right.Evaluate( newPivotItem );

                    if( newPivotItem.IsByName )
                        newPivotItem.parent[newPivotItem.name] = rightItem;
                    else
                        newPivotItem.parent[newPivotItem.index] = rightItem;
                }
            }
        }
    }
}