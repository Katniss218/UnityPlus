using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace UnityPlus.CSharp
{
    public class CSharpScript : MonoBehaviour
    {
        private static IEnumerable<string> GetAssemblyPaths()
        {
            // This directory should include the netstandard dll.
            return Directory.GetFiles( $"{Application.dataPath}", "*.dll", SearchOption.AllDirectories );
        }

        public static Assembly CompileCode( string code )
        {
            SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText( code );

            string assemblyName = Path.GetRandomFileName();
            List<MetadataReference> references = new List<MetadataReference>()
            {
                MetadataReference.CreateFromFile(typeof(UnityEngine.Object).Assembly.Location), // UnityEngine.CoreModule
                MetadataReference.CreateFromFile(typeof(UnityPlus.CSharp.CSharpScript).Assembly.Location),
            };

            foreach( var assemblyPath in GetAssemblyPaths() )
            {
                MetadataReference @ref = MetadataReference.CreateFromFile( assemblyPath );
                references.Add( @ref );
            }

            CSharpCompilation compilation = CSharpCompilation.Create(
                assemblyName,
                syntaxTrees: new[] { syntaxTree },
                references: references,
                options: new CSharpCompilationOptions( OutputKind.DynamicallyLinkedLibrary ) );

            using( var ms = new MemoryStream() )
            {
                EmitResult result = compilation.Emit( ms );

                if( !result.Success )
                {
                    IEnumerable<Diagnostic> failures = result.Diagnostics.Where( diagnostic =>
                         diagnostic.IsWarningAsError ||
                         diagnostic.Severity == DiagnosticSeverity.Error );

                    foreach( Diagnostic diagnostic in failures )
                    {
                        var lineSpan = diagnostic.Location.GetLineSpan(); // Get the linespan of the error
                        UnityEngine.Debug.LogWarning( $"Error at Line {lineSpan.StartLinePosition.Line}, Column {lineSpan.StartLinePosition.Character}: {diagnostic.GetMessage( CultureInfo.InvariantCulture )}" );
                    }

                    return null;
                }
                else
                {
                    ms.Seek( 0, SeekOrigin.Begin );
                    return Assembly.Load( ms.ToArray() );
                }
            }
        }
    }
}