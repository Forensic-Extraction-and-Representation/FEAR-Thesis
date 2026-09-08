using FEAR.Runtime.Domain;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using System.Text;
using static FEAR.CFEARParser;

namespace FEAR.CFEAR.Transpiler
{
    /// <summary>
    /// Transpiles a parsed FEAR collector (CFEAR) program into C# source code and compiles it in-memory.
    /// Inherits from <see cref="CFEARParserBaseVisitor{object}"/> to traverse the parse tree and generate C# code.
    /// </summary>
    public partial class CSharpTranspiler : CFEARParserBaseVisitor<object>
    {
        /// <summary>
        /// Stores the generated C# class wrapper code, with a placeholder for interpreter code.
        /// </summary>
        private StringBuilder _classWrapperOutput = new StringBuilder();

        /// <summary>
        /// Stores the generated C# code for the interpreter logic.
        /// </summary>
        private StringBuilder _interpreterCodeOutput = new StringBuilder();

        /// <summary>
        /// The name of the output assembly to be generated.
        /// </summary>
        private string AssemblyName = "";

        /// <summary>
        /// Transpiles a FEAR collector program into C# source code, compiles it, and returns the resulting assembly as a memory stream.
        /// </summary>
        /// <param name="FEARProgram">The parsed FEAR collector program (CFEAR AST root).</param>
        /// <param name="options">Compilation options and post-compile actions.</param>
        /// <returns>A <see cref="MemoryStream"/> containing the compiled assembly.</returns>
        public MemoryStream Transpile(FEARCollector FEARProgram, FEARCompilationRequest options)
        {
            // Visit the collector statement and all collector uses to generate code
            VisitCollectorStatement(FEARProgram.CFEARCtxRoot.collectorStatement());
            FEARProgram.CFEARCtxRoot.collectorUse().ToList().ForEach((use) => VisitCollectorUse(use));

            // Visit all block statements to generate interpreter logic
            foreach (BlockStatementContext block in FEARProgram.CFEARCtxRoot.blockStatement())
            {
                VisitBlockStatement(block);
            }

            MemoryStream outputStream = new MemoryStream();

            // Gather all loaded assemblies as references for compilation
            List<MetadataReference> References = new List<MetadataReference>();
            foreach (var item in AppDomain.CurrentDomain.GetAssemblies())
            {
                References.Add(MetadataReference.CreateFromFile(item.Location));
            }
            
            // Set up C# compilation options for a DLL
            CSharpCompilationOptions DefaultCompilationOptions =
                new CSharpCompilationOptions(outputKind: OutputKind.DynamicallyLinkedLibrary, platform: Platform.AnyCpu)
                .WithOverflowChecks(true).WithOptimizationLevel(OptimizationLevel.Release);

            // Combine the class wrapper and interpreter code, replacing the placeholder
            string combinedCode = _classWrapperOutput.ToString().Replace("@EXECUTE_CODE@", _interpreterCodeOutput.ToString());
            
            // Encode the combined source code as UTF-8
            string sourceCode = SourceText.From(combinedCode, Encoding.UTF8).ToString();

            // Create a compilation result object and invoke any post-compile actions
            FEARCompilationResult fcr = new FEARCompilationResult(sourceCode, options);
            options.CompilerOptions.PostCompileAction(fcr);

            // Parse the source code into a syntax tree
            var parsedSyntaxTree = Parse(sourceCode, "", CSharpParseOptions.Default.WithLanguageVersion(LanguageVersion.CSharp11));

            // Compile the syntax tree into an in-memory assembly
            var compilation = CSharpCompilation.Create(AssemblyName, new SyntaxTree[] { parsedSyntaxTree }, references: References, DefaultCompilationOptions);
            var result = compilation.Emit(outputStream);

            return outputStream;
        }

        /// <summary>
        /// Parses C# source code into a Roslyn syntax tree.
        /// </summary>
        /// <param name="text">The C# source code to parse.</param>
        /// <param name="filename">The filename for the syntax tree (optional).</param>
        /// <param name="options">C# parse options (optional).</param>
        /// <returns>A <see cref="SyntaxTree"/> representing the parsed code.</returns>
        private static SyntaxTree Parse(string text, string filename = "", CSharpParseOptions options = null)
        {
            var stringText = SourceText.From(text, Encoding.UTF8);
            return SyntaxFactory.ParseSyntaxTree(stringText, options, filename);
        }
    }
}
