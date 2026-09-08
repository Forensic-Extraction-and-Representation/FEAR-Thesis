namespace FEAR.Domain.Arguments
{
    /// <summary>
    /// The options for source compilation in FEAR.`
    /// </summary>
    public enum SourceCompilationEnum
    {
        // Only compile scripts, do not use precompiled libraries.
        CompileScripts,

        // Only use pre-compiled scripts
        Precompiled,

        // Use both scripts and precompiled libraries.
        Both
    }
}
