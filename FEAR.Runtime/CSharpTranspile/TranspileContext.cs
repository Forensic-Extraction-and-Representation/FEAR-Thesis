using FEAR.Runtime.Domain;

namespace FEAR.Runtime.CSharpTranspile
{
    /// <summary>
    /// Represents the context for a transpilation operation in the FEAR runtime.
    /// Holds the compilation request and flags indicating the mode of operation (e.g., single file or directory).
    /// Used to pass state and configuration between transpiler components during code generation and compilation.
    /// </summary>
    public class TranspileContext
    {
        /// <summary>
        /// The compilation request containing options, execution context, and file/output information.
        /// </summary>
        public FEARCompilationRequest CompilationRequest { get; set; }

        /// <summary>
        /// Indicates whether the transpilation is being performed on a directory of files (true) or a single file (false).
        /// </summary>
        public bool CompileDirectory { get; internal set; }
    }
}
