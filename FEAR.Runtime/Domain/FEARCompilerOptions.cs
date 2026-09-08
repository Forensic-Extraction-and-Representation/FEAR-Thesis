using FEAR.Domain.Arguments;
using FEAR.Runtime.Execution;

namespace FEAR.Runtime.Domain
{
    /// <summary>
    /// Represents the set of compiler options for the FEAR compilation process.
    /// Includes configuration for output directories, source language options, and post-compilation actions.
    /// </summary>
    public class FEARCompilerOptions
    {
        /// <summary>
        /// Gets or sets the directory where compiled FEAR libraries will be output.
        /// </summary>
        public string CompiledDirectory { get; set; } = Path.Combine(".", "FEAR-Lib");

        /// <summary>
        /// Gets or sets the compiler options for the IFEAR language.
        /// </summary>
        public FEARCompilerOption IFEAROption { get; set; } = new FEARCompilerOption(true, new FEARSourceOption());

        /// <summary>
        /// Gets or sets the compiler options for the CFEAR language.
        /// </summary>
        public FEARCompilerOption CFEAROption { get; set; } = new FEARCompilerOption(true, new FEARSourceOption());

        /// <summary>
        /// Gets or sets the compiler options for the GFEAR language.
        /// </summary>
        public FEARCompilerOption GFEAROption { get; set; } = new FEARCompilerOption(true, new FEARSourceOption());

        /// <summary>
        /// Gets or sets the compiler options for the RFEAR language.
        /// </summary>
        public FEARCompilerOption RFEAROption { get; set; } = new FEARCompilerOption(true, new FEARSourceOption());

        /// <summary>
        /// An action to be executed after compilation completes.
        /// </summary>
        public Action<FEARCompilationResult> PostCompileAction = (compileResult) => { };

        /// <summary>
        /// Creates a default set of compiler options based on the provided execution options and post-compilation action.
        /// </summary>
        /// <typeparam name="T">The type of <see cref="FearArguments"/> used for execution.</typeparam>
        /// <param name="fearCliOptions">The execution options containing argument values.</param>
        /// <param name="postCompileAction">The action to execute after compilation.</param>
        /// <returns>A configured <see cref="FEARCompilerOptions"/> instance.</returns>
        public static FEARCompilerOptions DefaultCompilerOptions<T>(FearExecutionOptions<T> fearCliOptions, Action<FEARCompilationResult> postCompileAction)
            where T : FearArguments
        {
            // Define which source compilation options should trigger compilation
            List<SourceCompilationEnum> compileOptions = new List<SourceCompilationEnum>() { SourceCompilationEnum.Both, SourceCompilationEnum.CompileScripts };
            var compilerOptions = new FEARCompilerOptions() { };
            
            // Set the output directory for compiled libraries
            compilerOptions.CompiledDirectory = Path.Combine(fearCliOptions.Arguments.GetWorkingDirectory(), "FEAR-Lib");

            // Enable or disable compilation for each language based on the provided options
            compilerOptions.IFEAROption.Compile = compileOptions.Contains(fearCliOptions.Arguments.SourceCompilationOption.Value);
            compilerOptions.CFEAROption.Compile = compileOptions.Contains(fearCliOptions.Arguments.SourceCompilationOption.Value);
            compilerOptions.GFEAROption.Compile = compileOptions.Contains(fearCliOptions.Arguments.SourceCompilationOption.Value);
            compilerOptions.RFEAROption.Compile = compileOptions.Contains(fearCliOptions.Arguments.SourceCompilationOption.Value);

            // Set the source options for each language, including directory and file extensions
            compilerOptions.IFEAROption.SourceOption = new FEARSourceOption() { FEARLanguage = "IFEAR", Directory = Path.Combine(fearCliOptions.Arguments.GetScriptDirectory(), "IFEAR"), FileExtensions = new List<string>() { ".ifear" } };
            compilerOptions.CFEAROption.SourceOption = new FEARSourceOption() { FEARLanguage = "CFEAR", Directory = Path.Combine(fearCliOptions.Arguments.GetScriptDirectory(), "CFEAR"), FileExtensions = new List<string>() { ".cfear" } };
            compilerOptions.GFEAROption.SourceOption = new FEARSourceOption() { FEARLanguage = "GFEAR", Directory = Path.Combine(fearCliOptions.Arguments.GetScriptDirectory(), "GFEAR"), FileExtensions = new List<string>() { ".gfear" } };
            compilerOptions.RFEAROption.SourceOption = new FEARSourceOption() { FEARLanguage = "RFEAR", Directory = Path.Combine(fearCliOptions.Arguments.GetScriptDirectory(), "RFEAR"), FileExtensions = new List<string>() { ".rfear" } };

            // Assign the post-compilation action
            compilerOptions.PostCompileAction = postCompileAction;

            return compilerOptions;
        }
    }
}
