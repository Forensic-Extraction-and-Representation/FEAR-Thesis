namespace FEAR.Domain.Infrastructure
{
    /// <summary>
    /// Provides configuration options for the FEAR resolver, which is responsible for discovering and loading
    /// modules, interpreters, collectors, and graph codifiers from precompiled assemblies.
    /// </summary>
    public class FEARResolverOptions
    {
        /// <summary>
        /// Gets or sets the directory path where precompiled assemblies are located.
        /// The resolver will scan this directory (and subdirectories) for assemblies to load.
        /// Default is "Precompiled".
        /// </summary>
        public string PreCompiledDirectory { get; set; } = "Precompiled";

        /// <summary>
        /// Gets or sets a value indicating whether the resolver should ignore the precompiled directory.
        /// If true, no assemblies will be loaded from <see cref="PreCompiledDirectory"/>.
        /// </summary>
        public bool IgnorePreCompiledDirectory { get; set; } = false;
    }
}
