using FEAR.Runtime.Domain;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using System.Text;
using static FEAR.IFEARParser;

namespace FEAR.IFEAR.Transpiler
{

    public partial class CSharpTranspiler : IFEARParserBaseVisitor<object>
    {
        private readonly static string _FEARDeclarationDisallowedMessage = "'FEAR' is a special keyword reserved for the builtin library and cannot be used as a variable name.";
        private readonly static string _FEARAssignmentDisallowedMessage = "'FEAR' is a special keyword reserved for the builtin library and cannot be assigned to.";

        private readonly static string _FEARLibraryIndexReferenceInvalidMessage = "An index reference is not allowed after the library reference.";
        private readonly static string _FEARModuleIndexReferenceInvalidMessage = "An index reference is not allowed after a module reference.";

        private readonly static string _FEARMissingModuleMessage = "Missing a module name after the library reference.";
        private readonly static string _FEARInvalidModuleMessage = "This FEAR module does not exist.";

        private readonly static string _FEARMissingModuleMemberMessage = "Missing a module member reference after module name.";
        private readonly static string _FEARInvalidModuleMemberMessage = "This FEAR module member does not exist.";

        private StringBuilder _classWrapperOutput = new StringBuilder();
        private StringBuilder _interpreterCodeOutput = new StringBuilder();
        private List<FEARErrorBase> _transpileErrors = new List<FEARErrorBase>();
        private string AssemblyName = "";
        public MemoryStream Transpile(FEARInterpreter FEARProgram, FEARCompilationRequest options)
        {
            VisitInterpreter_definition(FEARProgram.IFEARCtxRoot.interpreter_definition());
            var interpreter = FEARProgram.IFEARCtxRoot.interpreter_source();

            foreach (StatementContext block in interpreter.statement())
            {
                VisitStatement(block);
            }

            MemoryStream outputStream = new MemoryStream();

            // compile the C# in the _output variable to a memory stream
            List<MetadataReference> References = new List<MetadataReference>();
            foreach (var item in AppDomain.CurrentDomain.GetAssemblies())
            {
                References.Add(MetadataReference.CreateFromFile(item.Location));
            }
            
            CSharpCompilationOptions DefaultCompilationOptions =
                new CSharpCompilationOptions(outputKind: OutputKind.DynamicallyLinkedLibrary, platform: Platform.AnyCpu)
                .WithOverflowChecks(true).WithOptimizationLevel(OptimizationLevel.Release);

            // combine the class wrapper and the interpreter code
            string combinedCode = _classWrapperOutput.ToString().Replace("@EXECUTE_CODE@", _interpreterCodeOutput.ToString());
            
            // encode soucre code
            string sourceCode = SourceText.From(combinedCode, Encoding.UTF8).ToString();

            FEARCompilationResult fcr = new FEARCompilationResult(sourceCode, options);

            options.CompilerOptions.PostCompileAction(fcr);

            // CSharp options
            var parsedSyntaxTree = Parse(sourceCode, "", CSharpParseOptions.Default.WithLanguageVersion(LanguageVersion.CSharp11));

            // compilation
            var compilation = CSharpCompilation.Create(AssemblyName, new SyntaxTree[] { parsedSyntaxTree }, references: References, DefaultCompilationOptions);
            var result = compilation.Emit(outputStream);
            return outputStream;
        }

        private static SyntaxTree Parse(string text, string filename = "", CSharpParseOptions options = null)
        {
            var stringText = SourceText.From(text, Encoding.UTF8);
            return SyntaxFactory.ParseSyntaxTree(stringText, options, filename);
        }
    }
}
