namespace FEAR.Domain.Arguments
{
    /// <summary>
    /// The FearArguments class defines the base arguments for the FEAR Hosted Service.
    /// </summary>
    public abstract class FearArguments
    {
        /// <summary>
        /// The directory where this instance of the FEAR Hosted Service is running from.
        /// </summary>
        public Parameter<string> WorkingDirectory { get; set; }

        /// <summary>
        /// Local directory where precompiled scripts are stored.
        /// This is also the location where library repositories are synchronized to.
        /// </summary>
        public Parameter<string> PreCompiledDirectory { get; set; }

        /// <summary>
        /// Local directory where packaged libraries are extract to before being moved into relevant script directories for compilation and execution.
        /// </summary>
        public Parameter<string> PackageDirectory { get; set; }

        /// <summary>
        /// Local directory where source code of scripts are stored.
        /// </summary>
        public Parameter<string> ScriptDirectory { get; set; }

        public Parameter<string> LogsDirectory { get; set; }

        /// <summary>
        /// The root directory for the output of transpiled source code.
        /// </summary>
        public Parameter<string> TranspiledSourceOutputDirectory { get; set; }

        public virtual string GetWorkingDirectory()
        {
            return WorkingDirectory.IsSet && !string.IsNullOrEmpty(WorkingDirectory?.Value) ? WorkingDirectory.Value : Directory.GetCurrentDirectory();
        }

        /// <summary>
        /// The location of the precompiled scripts that are included by the FEAR Resolver when looking for assemblies and types to load.
        /// </summary>
        /// <returns></returns>
        public virtual string GetPreCompiledDirectory()
        {
            return Path.Combine(GetWorkingDirectory(), PreCompiledDirectory.IsSet && !string.IsNullOrEmpty(PreCompiledDirectory?.Value) ? PreCompiledDirectory.Value : "PreCompiled");
        }

        /// <summary>
        /// The location that is used as an intermediary for packaged libraries of FEAR scripts, which are bundled in a zip file for distribution.
        /// </summary>
        /// <returns></returns>
        public virtual string GetPackageDirectory()
        {
            return Path.Combine(GetWorkingDirectory(), PackageDirectory.IsSet && !string.IsNullOrEmpty(PackageDirectory?.Value) ? PackageDirectory.Value : "Packages");
        }

        /// <summary>
        /// The location that is used as the base for FEAR scripts to be stored by the Repository Manager when synchronizing repositories, and where the FEAR Resolver looks for scripts to compile and execute.
        /// </summary>
        /// <returns></returns>
        public virtual string GetScriptDirectory()
        {
            return Path.Combine(GetWorkingDirectory(), ScriptDirectory.IsSet && !string.IsNullOrEmpty(ScriptDirectory?.Value) ? ScriptDirectory.Value : "Scripts");
        }

        /// <summary>
        /// The location that is used for output of logs from the FEAR Hosted Service.
        /// </summary>
        /// <returns></returns>
        public virtual string GetLogsDirectory()
        {
            return Path.Combine(GetWorkingDirectory(), LogsDirectory.IsSet && !string.IsNullOrEmpty(LogsDirectory?.Value) ? LogsDirectory.Value : "Logs");
        }

        /// <summary>
        /// The location that is used for output of C# source code of the FEAR scripts
        /// </summary>
        /// <returns></returns>
        public virtual string GetTranspiledSourceOutputDirectory()
        {
            return Path.Combine(GetWorkingDirectory(), TranspiledSourceOutputDirectory.IsSet && !string.IsNullOrEmpty(TranspiledSourceOutputDirectory?.Value) ? TranspiledSourceOutputDirectory.Value : "TranspiledSource");
        }

        /// <summary>
        /// Paths to repositories of gfear/rfear scripts
        /// </summary>
        public FearRepositoryPaths SourceRepositoryPaths { get; set; } = new FearRepositoryPaths();
        /// <summary>
        /// Paths to precompiled libraries of FEAR scripts
        /// </summary>
        public FearRepositoryPaths PrecompiledLibraryPaths { get; set; } = new FearRepositoryPaths();

        /// <summary>
        /// Paths to packaged sources of FEAR scripts, which are bundled in a zip file for distribution.
        /// </summary>
        public List<string> PackagedSourcesPaths { get; set; } = new List<string>();

        /// <summary>
        /// The type of matching to use when comparing types in scripts.
        /// PropertyFallback is used to match based on properties if a 'strict' (Type field) match fails.
        /// Strict is used to only match based on the Type field.
        /// </summary>
        public Parameter<TypeMatchingEnum> TypeMatchingOption { get; set; }

        /// <summary>
        /// Defines whether Scripts Only, Precompiled Libraries Only, or Both should be used.
        /// </summary>
        public Parameter<SourceCompilationEnum> SourceCompilationOption { get; set; }


        /// <summary>
        /// The base namespace to use for the graph of this hosted service.
        /// </summary>
        public Parameter<string> NamespaceOption { get; set; }

        /// <summary>
        /// The abbreviation for the namespace to use for the graph of this hosted service.
        /// </summary>
        public Parameter<string> NamespaceAbbrevOption { get; set; }

        /// <summary>
        /// Defines whether an ontology should be generated for the scripts, and what format to write it in.
        /// This currently only supports a 'Turtle' format.
        /// </summary>
        public Parameter<string> OntologyOutputFormatOption { get; set; }

        /// <summary>
        /// Defines whether only the testing of compilation should be performed, without executing the scripts against artifacts.
        /// </summary>
        public Parameter<bool> TestCompileOption { get; set; }

        /// <summary>
        /// Used to define file paths (full or partial) for the C# of scripts to be displayed in the output.
        /// When this is set, the C# code for the scripts will be displayed in the output.
        /// </summary>
        public Parameter<string[]> DisplayTranspileOption { get; set; }

        /// <summary>
        /// Restricts compilation to a specific set of scripts by providing a list of folder paths for each type of compiled script.
        /// </summary>
        public FearRepositoryPaths RestrictTo { get; set; } = new FearRepositoryPaths();
        
        /// <summary>
        /// Indicates whether the transpiled source code should be output.
        /// </summary>
        public Parameter<bool> OutputTranspiledSource { get; set; }

    }
}
