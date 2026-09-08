using FEAR.Runtime.Compiler;
using FEAR.Runtime.Compiler.Transpiler;
using FEAR.Runtime.CSharpTranspile;
using FEAR.Runtime.Domain;
using FEAR.Runtime.TranspilerServices;
using FEAR.Runtime.TranspilerServices.Expressions;
using FEAR.Runtime.TranspilerServices.Variables;
using FEAR.Runtime.TranspilerHelpers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using System.Text;
using static FEAR.GFEARParser;

namespace FEAR.GFEAR.Transpiler
{
    /// <summary>
    /// Transpiles GFEAR graph codifier scripts into C# code for compilation and execution.
    ///
    /// The generated C# code implements graph codify scripts that process data queued for knowledge extraction.
    /// This data can be either:
    ///   - An artifact directly (raw evidence or input data)
    ///   - The result of a collector (CFEAR) script
    ///
    /// The transpiler ensures that all relevant data, regardless of its source, is processed
    /// by the appropriate graph codifiers for further transformation or enrichment in the FEAR pipeline.
    /// </summary>
    public partial class CSharpTranspiler : GFEARParserBaseVisitor<object>
    {
        private StringBuilder _classWrapperOutput = new StringBuilder();
        private StringBuilder _graphCodifierCodeOutput = new StringBuilder();
        private List<FEARErrorBase> _transpileErrors = new List<FEARErrorBase>();

        private AssemblyManager _assemblyManager = new AssemblyManager();
        private VariableManager _variableManager = new VariableManager();
        private EntityPropertyManager _entityPropertyManager = new EntityPropertyManager();
        private TranspilerExpressionContext _expressionContext = null;
        public GFEARTranspileContext Context { get; private set; }

        EntityTypeMeta CurrentEntityScopeType => Context.TypeManager.GetEntityType(_variableManager.Current.EntityName);

        /// <summary>
        /// Transpiles an array of FEARGraphCodifier objects into a compiled assembly stream.
        /// Each codifier is converted to a C# syntax tree, then compiled.
        /// The resulting assembly processes queued data (artifacts or collector results) for graph codification.
        /// </summary>
        public MemoryStream Transpile(FEARGraphCodifier[] grpahCodifiers, FEARCompilationRequest options, GFEARTranspileContext ctx)
        {
            Context = ctx;
            List<SyntaxTree> trees = new List<SyntaxTree>();
            foreach (var codifier in grpahCodifiers)
            {
                trees.Add(CompileToSyntaxTree(codifier, options));
            }

            return CSharpTranspileActions.CompileSyntaxTrees(new CSharpTranspileActions.SyntaxTreeCompileContext()
            {
                AssemblyManager = _assemblyManager,
                ParsedSyntaxTrees = trees.ToArray()
            }, options.TelemetrySignalService);
        }

        /// <summary>
        /// Transpiles a single FEARGraphCodifier object into a compiled assembly stream.
        /// The resulting assembly processes queued data (artifacts or collector results) for graph codification.
        /// </summary>
        public MemoryStream Transpile(FEARGraphCodifier FEARProgram, FEARCompilationRequest options, GFEARTranspileContext ctx)
        {
            Context = ctx;
            var parsedSyntaxTree = CompileToSyntaxTree(FEARProgram, options);
            return CSharpTranspileActions.CompileSyntaxTrees(new CSharpTranspileActions.SyntaxTreeCompileContext()
            {
                AssemblyManager = _assemblyManager,
                ParsedSyntaxTrees = new SyntaxTree[] { parsedSyntaxTree }
            }, options.TelemetrySignalService);
        }

        /// <summary>
        /// Compiles a FEARGraphCodifier into a C# syntax tree.
        /// The resulting code implements a graph codify script that processes queued data (artifact or collector result).
        /// </summary>
        private SyntaxTree CompileToSyntaxTree(FEARGraphCodifier FEARProgram, FEARCompilationRequest options)
        {
            CompilerTelemetry.SendCompilationTelemetrySignal($"Transpiling File - {options.FileName} {FEARProgram.SourceFileName}", options.TelemetrySignalService);
            _classWrapperOutput = new StringBuilder();
            _graphCodifierCodeOutput = new StringBuilder();
            VisitCodifierDefinitionBlock(FEARProgram.GFEARCtxRoot.codifierDefinitionBlock());
            ResultEntityBlockContext reBlock = FEARProgram.GFEARCtxRoot.resultEntityBlock();
            VisitResultEntityBlock(reBlock);

            // combine the class wrapper and the interpreter code
            string combinedCode = _classWrapperOutput.ToString().Replace("@EXECUTE_CODE@", _graphCodifierCodeOutput.ToString());
            string sourceCode = SourceText.From(combinedCode, Encoding.UTF8).ToString();

            // Write transpiled source if enabled
            TranspiledSourceWriter.WriteIfEnabled("gfear", FEARProgram.SourceFileName, sourceCode, options);

            // (Keep PostCompileAction for any other uses)
            FEARCompilationResult fcr = new FEARCompilationResult(sourceCode, new FEARCompilationRequest(options.CompilerOptions, options.ExecutionOptions, options.OntologyStore, options.TelemetrySignalService, FEARProgram.SourceFileName, options.OutFile));
            options.CompilerOptions.PostCompileAction(fcr);

            if (_transpileErrors.Count > 0)
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine($"Transpile Errors in {FEARProgram.SourceFileName}:");
                foreach (var error in _transpileErrors) sb.AppendLine(error.ToString());
                CompilerTelemetry.SendCompilationTelemetrySignal(sb.ToString(), options.TelemetrySignalService);
            }

            return CSharpTranspileActions.Parse(sourceCode, "", CSharpParseOptions.Default.WithLanguageVersion(LanguageVersion.CSharp11));
        }
    }
}
