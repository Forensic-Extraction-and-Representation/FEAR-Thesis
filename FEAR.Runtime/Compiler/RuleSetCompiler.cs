using Antlr4.Runtime;
using FEAR.RFEAR;
using FEAR.RFEAR.Transpiler;
using FEAR.Runtime.CSharpTranspile;
using FEAR.Runtime.Domain;
using Lucene.Net.QueryParsers.Flexible.Core.Nodes;

namespace FEAR.Runtime.Compiler
{
    /// <summary>
    /// Compiles RFEAR (Rule FEAR) ruleset source files or directories into .NET assemblies.
    /// Handles parsing, transpilation, and output for both single files and directories of RFEAR scripts.
    /// </summary>
    public class RuleSetCompiler
    {

        /// <summary>
        /// Compiles a RFEAR ruleset source file or directory as specified in the compilation request.
        /// If the input is a directory, compiles all matching files in the directory.
        /// </summary>
        /// <param name="options">The compilation request containing file/directory and output information.</param>
        public static void Compile(FEARCompilationRequest options)
        {
            TranspileContext ctx = new TranspileContext();
            ctx.CompileDirectory = false;
            ctx.CompilationRequest = options;

            if (Directory.Exists(ctx.CompilationRequest.FileName))
            {
                ctx.CompileDirectory = true;
                CompilerTelemetry.SendCompilationTelemetrySignal($"Compiling RFEAR rulesets in directory: {ctx.CompilationRequest.FileName}", ctx.CompilationRequest.TelemetrySignalService);

                CompileDirectory(ctx);
            }
            else if (File.Exists(ctx.CompilationRequest.FileName))
            {
                CompilerTelemetry.SendCompilationTelemetrySignal($"Compiling RFEAR ruleset file: {ctx.CompilationRequest.FileName}", ctx.CompilationRequest.TelemetrySignalService);

                Compile(File.OpenRead(ctx.CompilationRequest.FileName), ctx);
            }
            else
            {
                CompilerTelemetry.SendCompilationTelemetrySignal($"RFEAR ruleset source file or directory not found: {ctx.CompilationRequest.FileName}", ctx.CompilationRequest.TelemetrySignalService);
            }
        }

        /// <summary>
        /// Compiles a single RFEAR ruleset source file stream into a .NET assembly.
        /// </summary>
        /// <param name="file">The input stream for the RFEAR source file.</param>
        /// <param name="ctx">The transpile context containing compilation options.</param>
        public static void Compile(Stream file, TranspileContext ctx)
        {
            CompilerTelemetry.SendCompilationTelemetrySignal($"Parsing RFEAR File - {ctx.CompilationRequest.FileName}", ctx.CompilationRequest.TelemetrySignalService);
            RFEARLexer lexer = new RFEARLexer(new AntlrInputStream(file));
            CommonTokenStream tokens = new CommonTokenStream(lexer);
            RFEARParser parser = new RFEARParser(tokens);
            RFEARParser.ProgramContext tree = parser.program();

            var treeString = tree.ToStringTree(parser);

            // Compile to a .NET Assembly
            FEARRuleSet fearRuleSet = new FEARRuleSet(tree, new List<FEARErrorBase>(), ctx.CompilationRequest.FileName);
            MemoryStream assemblyMS = new CSharpTranspiler().Transpile(new[] { fearRuleSet }, ctx.CompilationRequest, ctx);

            // Write the assembly to the output file
            using (FileStream fs = new FileStream(ctx.CompilationRequest.OutFile, FileMode.OpenOrCreate))
            {
                assemblyMS.WriteTo(fs);
            }
        }

        /// <summary>
        /// Compiles all RFEAR ruleset source files in a directory into a single .NET assembly.
        /// </summary>
        /// <param name="ctx">The transpile context containing directory and compilation options.</param>
        public static void CompileDirectory(TranspileContext ctx)
        {
            List<FEARRuleSet> rulesets = new List<FEARRuleSet>();

            foreach (var ext in ctx.CompilationRequest.CompilerOptions.RFEAROption.SourceOption.FileExtensions)
            {
                foreach (var f in Directory.GetFiles(ctx.CompilationRequest.FileName, "*" + ext))
                {
                    try
                    {
                        CompilerTelemetry.SendCompilationTelemetrySignal($"Compiling RFEAR ruleset file: {f}", ctx.CompilationRequest.TelemetrySignalService);
                        RFEARLexer lexer = new RFEARLexer(new AntlrFileStream(f));
                        CommonTokenStream tokens = new CommonTokenStream(lexer);
                        RFEARParser parser = new RFEARParser(tokens);
                        RFEARParser.ProgramContext tree = parser.program();

                        var treeString = tree.ToStringTree(parser);
                        rulesets.Add(new FEARRuleSet(tree, new List<FEARErrorBase>(), f));
                    }
                    catch (Exception ex)
                    {

                    }
                }

                if (rulesets.Count > 0)
                {
                    CompilerTelemetry.SendCompilationTelemetrySignal($"Compiling {rulesets.Count} RFEAR rulesets into assembly: {ctx.CompilationRequest.OutFile}", ctx.CompilationRequest.TelemetrySignalService);
                    MemoryStream assemblyMS = new CSharpTranspiler().Transpile(rulesets.ToArray(), ctx.CompilationRequest, ctx);
                    using (FileStream fs = new FileStream(ctx.CompilationRequest.OutFile, FileMode.OpenOrCreate))
                    {
                        assemblyMS.WriteTo(fs);
                    }
                }
            }
        }
    }
}
