using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnityPlus.Serialization.Patching.DSL.SyntaxTree
{
    public class TransformationBody
    {
        public Statement[] Statements { get; set; }

        public void Invoke( IEnumerable<TrackedSerializedData> pivot )
        {
            foreach( var statement in Statements )
            {
                statement.Invoke( pivot );
            }
        }
    }
}