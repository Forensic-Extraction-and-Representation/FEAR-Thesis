namespace FEAR.Runtime.TranspilerServices
{
    /// <summary>
    /// Manages assembly-related metadata for transpilation and code generation.
    /// Used to track the name of the dynamically generated or compiled assembly.
    /// </summary>
    public class AssemblyManager
    {
        /// <summary>
        /// The name of the assembly being generated.
        /// </summary>
        public string AssemblyName { get; set; } = "";
    }
}
