namespace FEAR.Domain.Infrastructure
{
    /// <summary>
    /// Represents an entry for a temporary directory associated with a provenance object.
    /// Used by <see cref="ITemporaryDirectoryManager"/> to track the lifecycle and usage of temporary directories
    /// during evidence processing, analysis, or transformation workflows.
    /// </summary>
    public class TemporaryDirectoryEntry
    {
        /// <summary>
        /// Gets or sets the provenance URI associated with this temporary directory.
        /// This links the directory to a specific provenance object or evidence item.
        /// </summary>
        public string ProvenanceUri { get; set; }

        /// <summary>
        /// Gets or sets the file system path of the temporary directory.
        /// </summary>
        public string Path { get; set; }

        /// <summary>
        /// Gets or sets the date and time when the temporary directory was created.
        /// </summary>
        public DateTime Created { get; set; }

        /// <summary>
        /// Gets or sets the date and time when the temporary directory was last used.
        /// Useful for cleanup and lifecycle management.
        /// </summary>
        public DateTime LastUsed { get; set; }
    }
}
