using Antlr4.Runtime;
using FEAR.GFEAR;
using FEAR.GFEAR.Transpiler;
using FEAR.Runtime.Domain;
using FEAR.Runtime.GFEAR;
using VDS.RDF.Ontology;

namespace FEAR.Runtime.Compiler
{
    /// <summary>
    /// Compiles GFEAR (Graph FEAR) source files or directories into .NET assemblies and optionally generates ontology files.
    /// Handles parsing, transpilation, and output for both single files and directories of GFEAR scripts.
    /// </summary>
    public class GraphCodifierCompiler
    {
        /// <summary>
        /// Compiles a GFEAR source file or directory as specified in the compilation request.
        /// If the input is a directory, compiles all matching files in the directory.
        /// </summary>
        /// <param name="options">The compilation request containing file/directory and output information.</param>
        public static void Compile(FEARCompilationRequest options)
        {
            GFEARTranspileContext ctx = new GFEARTranspileContext();
            ctx.CompileDirectory = false;
            ctx.CompilationRequest = options;

            if (Directory.Exists(ctx.CompilationRequest.FileName))
            {
                // If the input is a directory, set the context to compile the directory
                ctx.CompileDirectory = true;
                CompileDirectory(ctx);
            }
            else
            {
                // If the input is a single file, compile that file
                if (File.Exists(ctx.CompilationRequest.FileName))
                {
                    Compile(File.OpenRead(ctx.CompilationRequest.FileName), ctx);
                }
            }
        }

        /// <summary>
        /// Compiles a single GFEAR source file stream into a .NET assembly.
        /// </summary>
        /// <param name="file">The input stream for the GFEAR source file.</param>
        /// <param name="ctx">The transpile context containing compilation options.</param>
        public static void Compile(Stream file, GFEARTranspileContext ctx)
        {
            CompilerTelemetry.SendCompilationTelemetrySignal($"Parsing GFEAR File - {ctx.CompilationRequest.FileName}", ctx.CompilationRequest.TelemetrySignalService);
            FEARLexer lexer = new FEARLexer(new AntlrInputStream(file));
            CommonTokenStream tokens = new CommonTokenStream(lexer);
            GFEARParser parser = new GFEARParser(tokens);
            GFEARParser.ProgramContext tree = parser.program();

            var treeString = tree.ToStringTree(parser);

            // Compile to a .NET Assembly
            FEARGraphCodifier gfearScript = new FEARGraphCodifier(tree, new List<FEARErrorBase>(), ctx.CompilationRequest.FileName);
            MemoryStream assemblyMS = new CSharpTranspiler().Transpile(gfearScript, ctx.CompilationRequest, ctx);

            // Write the assembly to the output file
            using (FileStream fs = new FileStream(ctx.CompilationRequest.OutFile, FileMode.OpenOrCreate))
            {
                assemblyMS.WriteTo(fs);
            }
        }

        /// <summary>
        /// Compiles all GFEAR source files in a directory into a single .NET assembly.
        /// Optionally generates an ontology file if requested in the execution options.
        /// </summary>
        /// <param name="ctx">The transpile context containing directory and compilation options.</param>
        public static void CompileDirectory(GFEARTranspileContext ctx)
        {
            List<FEARGraphCodifier> codifiers = new List<FEARGraphCodifier>();

            foreach (var ext in ctx.CompilationRequest.CompilerOptions.GFEAROption.SourceOption.FileExtensions)
            {
                foreach (var f in Directory.GetFiles(ctx.CompilationRequest.FileName, "*" + ext))
                {
                    try
                    {
                        CompilerTelemetry.SendCompilationTelemetrySignal($"Parsing GFEAR File - {f}", ctx.CompilationRequest.TelemetrySignalService);
                        FEARLexer lexer = new FEARLexer(new AntlrFileStream(f));
                        CommonTokenStream tokens = new CommonTokenStream(lexer);
                        GFEARParser parser = new GFEARParser(tokens);
                        GFEARParser.ProgramContext tree = parser.program();

                        var treeString = tree.ToStringTree(parser);
                        codifiers.Add(new FEARGraphCodifier(tree, new List<FEARErrorBase>(), f));
                    }
                    catch (Exception ex)
                    {

                    }
                }

                if (codifiers.Count > 0)
                {
                    MemoryStream assemblyMS = new CSharpTranspiler().Transpile(codifiers.ToArray(), ctx.CompilationRequest, ctx);
                    using (FileStream fs = new FileStream(ctx.CompilationRequest.OutFile, FileMode.OpenOrCreate))
                    {
                        assemblyMS.WriteTo(fs);
                    }
                }
            }

            // Optionally generate an ontology file if requested
            if (ctx.CompilationRequest.ExecutionOptions.Arguments?.OntologyOutputFormatOption?.IsSet ?? false)
            {
                var otGen = new OntologyGenerator();
                OntologyGraph og = otGen.GenerateOntology(ctx);

                ctx.CompilationRequest.OntologyStore.Merge(og);
                string filePath = "";

                if (ctx.CompileDirectory)
                {
                    string folder = ctx.CompilationRequest.FileName;
                    string filename = "Ontology.ttl";

                    filePath = Path.Combine(folder, filename);
                }
                else
                {
                    filePath = Path.ChangeExtension(ctx.CompilationRequest.FileName, "ttl");
                }

                using (FileStream fs = new FileStream(filePath, FileMode.OpenOrCreate))
                {
                    fs.SetLength(0);
                    using (StreamWriter sw = new StreamWriter(fs))
                        otGen.WriteTo(sw, og);

                    otGen.WriteTo(Console.Out, og);
                }
            }
        }
    }
}
