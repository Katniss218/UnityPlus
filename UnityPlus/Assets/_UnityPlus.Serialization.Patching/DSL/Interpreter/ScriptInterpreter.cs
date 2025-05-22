using System.Collections.Generic;
using System;
using UnityPlus.Serialization.Patching.DSL.SyntaxTree;

namespace UnityPlus.Serialization.Patching.DSL.Interpreter
{
    public class ScriptInterpreter
    {
        private readonly DataFixerScript _script;

        private TrackedSerializedData _rootPivot;
        private List<TrackedSerializedData> _currentPivots = new();

        private readonly Stack<Dictionary<string, List<SerializedData>>> _variablesInScopes = new();

        private readonly Dictionary<string, FunctionDefinition> _functions = new();

        public ScriptInterpreter( DataFixerScript script, SerializedData data )
        {
            _script = script;

            // init pivots.
        }

        public void Execute()
        {
            foreach( var statement in _script.Statements )
            {
                switch( statement )
                {
                    //statement.Invoke( _currentPivots );

                    case Transformation transformation:

                        _variablesInScopes.Push( new Dictionary<string, List<SerializedData>>() );

                        var newPivots = new List<TrackedSerializedData>( ResolvePath( transformation.Path, pivots ) );
                        if( !string.IsNullOrEmpty( transformation.VarName ) )
                        {
                            _variablesInScopes.Peek()[transformation.VarName] = newPivots;
                        }
#warning TODO - I would like the syntax nodes to either have accompanying class for execution, or execute themselves, but without storing the state.
                        // that is, the interpreter shouldn't hardcode the behavior.

                        Execute( statement.Body, newPivots );

                        _variablesInScopes.Pop();
                        break;

                    case AssignmentStatement assign:
                        Assign( assign.LeftPath, assign.Right, pivots );
                        break;

                    default:
                        throw new NotSupportedException( $"Unsupported AST node: {statement.GetType()}" );
                }
            }
        }
    }
}