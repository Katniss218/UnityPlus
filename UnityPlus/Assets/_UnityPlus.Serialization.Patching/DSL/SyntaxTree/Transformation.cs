using System.Collections.Generic;

namespace UnityPlus.Serialization.Patching.DSL.SyntaxTree
{
    /// <summary>
    /// A transformation is the basic unit of work performed by a script. Kind of like a function call, or a sequence of statements.
    /// </summary>
    /// <example>
    /// 
    ///     (FOR any WHERE this == "literal")
    ///     {
    ///         this = null;
    ///     }
    /// 
    /// </example>
    public class Transformation : IStatement
    {
        public TransformationHeader[] Headers { get; set; }

        public IStatement[] Body;

        public void Invoke( IEnumerable<TrackedSerializedData> pivot )
        {
            foreach( var header in Headers )
            {
                pivot = header.Invoke( pivot );
            }

            foreach( var statement in Body )
            {
                statement.Invoke( pivot );
            }
        }
    }
}