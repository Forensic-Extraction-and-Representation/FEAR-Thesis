namespace FEAR.Runtime.Domain
{
    /// <summary>
    /// Represents compiler options for the FEAR compilation process.
    /// </summary>
    public class FEARCompilerOption
    {
        /// <summary>
        /// Gets or sets a value indicating whether compilation should be performed.
        /// </summary>
        public bool Compile { get; set; }

        /// <summary>
        /// Gets or sets the source options used for the compilation process.
        /// </summary>
        public FEARSourceOption SourceOption { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="FEARCompilerOption"/> class.
        /// </summary>
        /// <param name="compile">Indicates whether compilation should be performed.</param>
        /// <param name="sourceOption">The source options for the compilation process.</param>
        public FEARCompilerOption(bool compile, FEARSourceOption sourceOption)
        {
            Compile = compile;
            SourceOption = sourceOption;
        }
    }
}