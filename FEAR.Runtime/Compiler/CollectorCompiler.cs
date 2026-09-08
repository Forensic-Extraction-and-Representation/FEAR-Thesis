using Antlr4.Runtime;
using FEAR.CFEAR;
using FEAR.CFEAR.Transpiler;
using FEAR.Runtime.Domain;

namespace FEAR.Runtime.Compiler
{
    public class CollectorCompiler
    {
        public static void Compile(FEARCompilationRequest options)
        {
            if (File.Exists(options.FileName))
            {
                Compile(File.OpenRead(options.FileName), options);
            }
        }

        public static void Compile(Stream file, FEARCompilationRequest options) { 
            FEARLexer lexer = new FEARLexer(new AntlrInputStream(file));
            CommonTokenStream tokens = new CommonTokenStream(lexer);
            CFEARParser parser = new CFEARParser(tokens);
            CFEARParser.ProgramContext tree = parser.program();

            var treeString = tree.ToStringTree(parser);
            Console.WriteLine(treeString);

            // Compile to a .NET Assembly
            MemoryStream assemblyMS = new CSharpTranspiler().Transpile(new FEARCollector(tree, new List<FEARErrorBase>()), options);

            // write the assembly to the outfile
            using (FileStream fs = new FileStream(options.OutFile, FileMode.Create))
            {
                assemblyMS.WriteTo(fs);
            }
        }
    }
}
