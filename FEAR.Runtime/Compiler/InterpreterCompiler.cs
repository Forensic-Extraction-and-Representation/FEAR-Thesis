using Antlr4.Runtime;
using FEAR.Domain.Interpreter;
using FEAR.IFEAR;
using FEAR.Runtime.Domain;

namespace FEAR.Runtime.Compiler
{
    /// <summary>
    /// Provides static methods to compile and execute FEAR interpreter source files.
    /// Handles parsing, transpilation, and dynamic execution of interpreter logic.
    /// </summary>
    public class InterpreterCompiler
    {
        /// <summary>
        /// Compiles a FEAR interpreter source file into a .NET assembly.
        /// Parses the source, generates C# code, and writes the compiled assembly to the specified output file.
        /// </summary>
        /// <param name="options">The compilation request containing source and output information.</param>
        public static void Compile(FEARCompilationRequest options)
        {
            FEARLexer lexer = new FEARLexer(new AntlrFileStream(options.FileName));
            CommonTokenStream tokens = new CommonTokenStream(lexer);
            IFEARParser parser = new IFEARParser(tokens);
            IFEARParser.ProgramContext tree = parser.program();

            var treeString = tree.ToStringTree(parser);
            Console.WriteLine(treeString);

            // Compile to a .NET Assembly
            MemoryStream assemblyMS = new IFEAR.Transpiler.CSharpTranspiler().Transpile(new FEARInterpreter(tree, new List<FEARErrorBase>()), options);
            
            // Write the assembly to the output file
            using (FileStream fs = new FileStream(options.OutFile, FileMode.Create))
            {
                assemblyMS.WriteTo(fs);
            }
        }

        /// <summary>
        /// Executes a compiled FEAR interpreter on the provided data stream.
        /// </summary>
        /// <param name="data">The input data as a memory stream.</param>
        /// <param name="interpreter">The interpreter instance to execute.</param>
        /// <returns>The return values produced by the interpreter execution.</returns>
        public static object Execute(MemoryStream data, IFEARInterpreter interpreter)
        {
            InterpreterContext context = new InterpreterContext(interpreter);
            context.Execute(data);

            return context.ReturnValues;
        }
    }
}
