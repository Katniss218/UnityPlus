using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnityPlus.Serialization.Patching.DSL.SyntaxTree
{
    /// <summary>
    /// A transformation is the basic unit of work performed by a script. Kind of like a function call, or a sequence of statements.
    /// </summary>
    public class Transformation : IStatementOrTransformation
    {
        public TransformationHeader[] Headers { get; set; }

        public TransformationBody Body;

        public void Invoke( IEnumerable<TrackedSerializedData> pivot )
        {
            foreach( var header in Headers )
            {
                pivot = header.Invoke( pivot );
            }

            Body.Invoke( pivot );
        }
    }
}