using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnityPlus.Serialization.Patching.DSL.SyntaxTree
{
    public class FunctionInvocation : IExpression
    {
        public string FunctionName { get; }

        public IExpression[] Arguments { get; }

        public SerializedData Evaluate( TrackedSerializedData pivotItem )
        {
            SerializedData[] argumentValues = new SerializedData[Arguments.Length];

            for( int i = 0; i < Arguments.Length; i++ )
            {
                argumentValues[i] = Arguments[i].Evaluate( pivotItem );
            }

            return FunctionLibrary.Invoke( FunctionName, argumentValues );
        }
    }
}