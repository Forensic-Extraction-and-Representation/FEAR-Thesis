using FEAR.Domain.Provenance;

namespace FEAR.Domain.Infrastructure
{
    /// <summary>
    /// Defines a contract for managing temporary directories associated with provenance objects.
    /// Implementations are responsible for creating, tracking, and cleaning up temporary directories
    /// used during evidence processing, analysis, or transformation workflows.
    /// Temporary directories are typically used where an evidence item is required to be locally stored for processing.
    /// </summary>
    public interface ITemporaryDirectoryManager
    {
        /// <summary>
        /// Retrieves or creates a <see cref="TemporaryDirectoryEntry"/> for the specified provenance object.
        /// This entry represents a temporary directory associated with the provenance, used for storing intermediate or working files.
        /// </summary>
        /// <param name="prov">The provenance object for which to get or create a temporary directory entry.</param>
        /// <returns>The <see cref="TemporaryDirectoryEntry"/> associated with the provenance.</returns>
        TemporaryDirectoryEntry GetTemporaryDirectoryEntry(IProvenance prov);

        /// <summary>
        /// Cleans up all managed temporary directories, removing any that are no longer needed.
        /// This is typically called at the end of a processing workflow or when temporary storage should be released.
        /// </summary>
        void CleanupTemporaryDirectory();

        /// <summary>
        /// Removes a specific temporary directory entry and deletes the associated directory from the file system.
        /// </summary>
        /// <param name="entry">The temporary directory entry to remove and clean up.</param>
        void RemoveTemporaryDirectoryEntry(TemporaryDirectoryEntry entry);
    }
}
