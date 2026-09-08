using FEAR.Domain.Ontology;
using FEAR.Domain.Telemetry;
using FEAR.Runtime.Execution;

namespace FEAR.Runtime.Domain
{
    /// <summary>
    /// Represents a request to compile a FEAR script or project, including all necessary options and file paths.
    /// </summary>
    public class FEARCompilationRequest
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FEARCompilationRequest"/> class.
        /// </summary>
        /// <param name="options">Compiler options to use for the compilation process.</param>
        /// <param name="executionOptions">Execution options specifying runtime behavior.</param>
        /// <param name="filename">The input file name to be compiled.</param>
        /// <param name="outFile">The output file name for the compiled result.</param>
        public FEARCompilationRequest(FEARCompilerOptions options, FearExecutionOptions executionOptions, IFEAROntologyStore ontologyStore, IFEARTelemetrySignalService telemetrySignalService, string filename, string outFile)
        {
            CompilerOptions = options;
            ExecutionOptions = executionOptions;
            FileName = filename;
            OutFile = outFile;
            OntologyStore = ontologyStore;
            TelemetrySignalService = telemetrySignalService;
        }

        /// <summary>
        /// Gets the execution options specifying how the compiled code should be executed.
        /// </summary>
        public FearExecutionOptions ExecutionOptions { get; }

        /// <summary>
        /// Gets the compiler options used for the compilation process.
        /// </summary>
        public FEARCompilerOptions CompilerOptions { get; }

        public IFEAROntologyStore OntologyStore { get; }

        public IFEARTelemetrySignalService TelemetrySignalService { get; }

        /// <summary>
        /// Gets or sets the input file name to be compiled.
        /// </summary>
        public string FileName { get; set; }

        /// <summary>
        /// Gets or sets the output file name for the compiled result.
        /// </summary>
        public string OutFile { get; set; }
    }
}
