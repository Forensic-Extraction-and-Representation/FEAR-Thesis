namespace FEAR.Runtime.Domain
{
    /// <summary>
    /// Represents the result of a FEAR compilation process.
    /// Contains the generated source code and the associated compilation request.
    /// </summary>
    public class FEARCompilationResult
    {
        /// <summary>
        /// Gets the generated source code from the compilation process.
        /// </summary>
        public string SourceCode { get; private set; }

        /// <summary>
        /// Gets the associated compilation request that produced this result.
        /// </summary>
        public FEARCompilationRequest CompilationRequest { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="FEARCompilationResult"/> class.
        /// </summary>
        /// <param name="sourceCode">The generated source code.</param>
        /// <param name="compilationRequest">The associated compilation request.</param>
        public FEARCompilationResult(string sourceCode, FEARCompilationRequest compilationRequest)
        {
            SourceCode = sourceCode;
            CompilationRequest = compilationRequest;
        }
    }
}
