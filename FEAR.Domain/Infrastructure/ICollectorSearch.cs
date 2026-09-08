using FEAR.Domain.Collector;

namespace FEAR.Domain.Infrastructure
{
    /// <summary>
    /// Defines parameters for the search details of a Collector.
    /// Implementations specify how a collector searches for files and/or folders, and what type of items it operates on.
    /// </summary>
    public interface ICollectorSearch
    {
        /// <summary>
        /// Gets the wildcard or pattern used to search for folders.
        /// </summary>
        string FolderSearch { get; }

        /// <summary>
        /// Gets the wildcard or pattern used to search for files.
        /// </summary>
        string FileSearch { get; }

        /// <summary>
        /// Gets the type of items the collector operates on (e.g., file, directory, stream, result object).
        /// </summary>
        CollectorTypeEnum CollectorType { get; }
    }
}
