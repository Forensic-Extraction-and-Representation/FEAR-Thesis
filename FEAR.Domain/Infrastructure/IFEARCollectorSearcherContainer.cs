using FEAR.Domain.Collector;

namespace FEAR.Domain.Infrastructure
{
    /// <summary>
    /// Defines a contract for managing and searching collector searchers in the FEAR framework.
    /// This interface allows registration of searchers for collectors and supports matching file or folder paths
    /// against registered collector search patterns using wildcards and regular expressions.
    /// </summary>
    public interface IFEARCollectorSearcherContainer
    {
        /// <summary>
        /// Registers a searcher for a collector, associating it with the collector's attribute.
        /// The searcher instance defines how the collector searches for files and/or folders.
        /// </summary>
        /// <param name="attribute">The collector attribute containing metadata such as the full name.</param>
        /// <param name="searcherInstance">The searcher instance implementing <see cref="ICollectorSearch"/>.</param>
        void AddSearcherForCollector(FEARCollectorAttribute attribute, ICollectorSearch searcherInstance);

        /// <summary>
        /// Attempts to find a registered collector whose search patterns match the provided path and collector type.
        /// Returns the full name of the matching collector, or null if no match is found.
        /// </summary>
        /// <param name="name">The file or folder path to match.</param>
        /// <param name="collectorTypeEnum">The collector type to match against (e.g., file, directory, stream).</param>
        /// <returns>The full name of the matching collector, or null if no match is found.</returns>
        string TryGetCollectorForPath(string name, CollectorTypeEnum collectorTypeEnum);
    }
}
