using FEAR.GFEAR;
using FEAR.Runtime.Compiler;
using FEAR.Runtime.Domain;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using Microsoft.CSharp.RuntimeBinder;
using System.Linq.Expressions;
using System.Text;

namespace FEAR.Runtime.CSharpTranspile
{
    /// <summary>
    /// Provides static actions for compiling and parsing C# syntax trees during transpilation.
    /// Used to generate in-memory .NET assemblies from dynamically generated C# code.
    /// </summary>
    public partial class CSharpTranspileActions
    {
        /// <summary>
        /// Compiles the provided syntax trees into a .NET assembly and returns the result as a memory stream.
        /// Adds all currently loaded assemblies and key .NET types as metadata references.
        /// </summary>
        /// <param name="compileContext">The context containing syntax trees and assembly information.</param>
        /// <returns>A <see cref="MemoryStream"/> containing the compiled assembly.</returns>
        public static MemoryStream CompileSyntaxTrees(SyntaxTreeCompileContext compileContext, FEAR.Domain.Telemetry.IFEARTelemetrySignalService telemetrySignalService)
        {
            MemoryStream outputStream = new MemoryStream();

            // Gather references from all loaded assemblies and key .NET types
            List<MetadataReference> References = new List<MetadataReference>();
            foreach (var item in AppDomain.CurrentDomain.GetAssemblies())
            {
                if (!string.IsNullOrEmpty(item.Location))
                    References.Add(MetadataReference.CreateFromFile(item.Location));
            }

            References.Add(MetadataReference.CreateFromFile(typeof(Uri).Assembly.Location));
            References.Add(MetadataReference.CreateFromFile(typeof(CSharpArgumentInfo).Assembly.Location));
            References.Add(MetadataReference.CreateFromFile(typeof(BinaryExpression).Assembly.Location));

            // Set up compilation options for a debug DLL
            CSharpCompilationOptions DefaultCompilationOptions =
                new CSharpCompilationOptions(outputKind: OutputKind.DynamicallyLinkedLibrary, platform: Platform.AnyCpu)
                .WithOverflowChecks(true).WithOptimizationLevel(OptimizationLevel.Debug);

            // Compile the syntax trees into the output stream
            var compilation = CSharpCompilation.Create(compileContext.AssemblyManager.AssemblyName, compileContext.ParsedSyntaxTrees, references: References, DefaultCompilationOptions);
            var result = compilation.Emit(outputStream);

            // Output diagnostics if compilation fails
            if (!result.Success)
            {
                foreach (var r in result.Diagnostics)
                {
                    CompilerTelemetry.SendCompilationTelemetrySignal(r.ToString(), telemetrySignalService);
                }
            }

            return outputStream;
        }

        /// <summary>
        /// Parses C# source code into a Roslyn syntax tree.
        /// </summary>
        /// <param name="text">The C# source code to parse.</param>
        /// <param name="filename">The filename for the syntax tree (optional).</param>
        /// <param name="options">C# parse options (optional).</param>
        /// <returns>A <see cref="SyntaxTree"/> representing the parsed code.</returns>
        public static SyntaxTree Parse(string text, string filename = "", CSharpParseOptions options = null)
        {
            var stringText = SourceText.From(text, Encoding.UTF8);
            return SyntaxFactory.ParseSyntaxTree(stringText, options, filename);
        }
    }
}
