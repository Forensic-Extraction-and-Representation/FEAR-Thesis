using FEAR.Runtime.TranspilerServices;
using Microsoft.CodeAnalysis;

namespace FEAR.Runtime.CSharpTranspile
{
    /// <summary>
    /// Represents the context required for compiling C# syntax trees into an assembly during transpilation.
    /// Holds the assembly metadata and the parsed syntax trees to be compiled.
    /// </summary>
    public partial class CSharpTranspileActions
    {
        /// <summary>
        /// Contains the assembly manager and the set of parsed syntax trees for compilation.
        /// </summary>
        public class SyntaxTreeCompileContext
        {
            /// <summary>
            /// Manages the name and metadata of the assembly being generated.
            /// </summary>
            public AssemblyManager AssemblyManager { get; set; }

            /// <summary>
            /// The array of parsed C# syntax trees to be compiled into the assembly.
            /// </summary>
            public SyntaxTree[] ParsedSyntaxTrees { get; set; }
        }
    }
}
