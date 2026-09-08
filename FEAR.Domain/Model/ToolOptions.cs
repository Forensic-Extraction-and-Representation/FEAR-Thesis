namespace FEAR.Domain.Model
{
    /// <summary>
    /// Represents configuration options for a forensic or analysis tool, including executable details, arguments, and input/output requirements.
    /// </summary>
    public class ToolOptions
    {
        /// <summary>
        /// Gets or sets the unique identifier for this tool option configuration.
        /// </summary>
        public virtual Guid ToolOptionId { get; set; }

        /// <summary>
        /// Gets or sets the name of the tool.
        /// </summary>
        public string ToolName { get; set; }

        /// <summary>
        /// Gets or sets the directory where the tool executable is located.
        /// </summary>
        public string ToolDirectory { get; set; }

        /// <summary>
        /// Gets or sets the name of the tool executable file.
        /// </summary>
        public string ToolExecutable { get; set; }

        /// <summary>
        /// Gets or sets the command-line arguments to be passed to the tool executable.
        /// </summary>
        public string ToolArguments { get; set; } = "";

        /// <summary>
        /// Gets or sets the placeholder in the arguments string that will be replaced with the input file path (default: "%F").
        /// </summary>
        public string InputPlaceholder { get; set; } = "%F";

        /// <summary>
        /// Gets or sets a value indicating whether the tool requires an output directory to be specified.
        /// </summary>
        public bool RequiresOutputDirectory { get; set; } = false;

        /// <summary>
        /// Gets or sets the placeholder in the arguments string that will be replaced with the output directory path (default: "%D").
        /// </summary>
        public string OutputPlaceholder { get; set; } = "%D";

        /// <summary>
        /// Gets or sets a value indicating whether the tool produces a file as output.
        /// </summary>
        public bool IsFileOutput { get; set; } = false;

        /// <summary>
        /// Gets or sets the name of the output file produced by the tool, if applicable.
        /// </summary>
        public string FileOuputName { get; set; } = "";
    }
}
