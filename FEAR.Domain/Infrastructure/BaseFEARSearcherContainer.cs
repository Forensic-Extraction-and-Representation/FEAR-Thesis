using FEAR.Domain.Collector;
using System.Text.RegularExpressions;

namespace FEAR.Domain.Infrastructure
{
    /// <summary>
    /// Provides a container for managing and searching collector searchers in the FEAR framework.
    /// This class allows registration of searchers for collectors and supports matching file or folder paths
    /// against registered collector search patterns using wildcards and regular expressions.
    /// </summary>
    public class BaseFEARSearcherContainer : IFEARCollectorSearcherContainer
    {
        /// <summary>
        /// Internal context for each registered collector searcher, holding the search patterns and compiled regex.
        /// </summary>
        private class CollectorSearchContext
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="CollectorSearchContext"/> class.
            /// Compiles the file and folder search patterns into regular expressions.
            /// </summary>
            /// <param name="collectorSearch">The collector search definition.</param>
            public CollectorSearchContext(ICollectorSearch collectorSearch)
            {
                CollectorSearch = collectorSearch;
                FileSearchRegex = collectorSearch.CollectorType.HasFlag(Collector.CollectorTypeEnum.File) ? ConvertFileWildcardSearchToRegex(collectorSearch.FileSearch) : null;
                FolderSearchRegex = ConvertDirectoryWildcardSearchToRegex(collectorSearch.FolderSearch, FileSearchRegex == null);
            }

            /// <summary>
            /// Gets the compiled regex for folder search, or null if not applicable.
            /// </summary>
            public Regex FolderSearchRegex { get; }

            /// <summary>
            /// Gets the compiled regex for file search, or null if not applicable.
            /// </summary>
            public Regex FileSearchRegex { get; }

            /// <summary>
            /// Gets the original collector search definition.
            /// </summary>
            public ICollectorSearch CollectorSearch { get; }
        }

        /// <summary>
        /// Index of registered collector searchers by their full name.
        /// </summary>
        private Dictionary<string, CollectorSearchContext> searcherIndex = new Dictionary<string, CollectorSearchContext>();

        /// <summary>
        /// Initializes a new instance of the <see cref="BaseFEARSearcherContainer"/> class.
        /// </summary>
        public BaseFEARSearcherContainer() { }

        /// <summary>
        /// Registers a searcher for a collector, associating it with the collector's attribute full name.
        /// </summary>
        /// <param name="attribute">The collector attribute containing the full name.</param>
        /// <param name="searcherInstance">The searcher instance implementing <see cref="ICollectorSearch"/>.</param>
        public void AddSearcherForCollector(FEARCollectorAttribute attribute, ICollectorSearch searcherInstance)
        {
            searcherIndex.Add(attribute.FullName, new CollectorSearchContext(searcherInstance));
        }

        /// <summary>
        /// Attempts to find a registered collector whose search patterns match the provided path and collector type.
        /// Returns the full name of the matching collector, or null if no match is found.
        /// </summary>
        /// <param name="name">The file or folder path to match.</param>
        /// <param name="collectorTypeEnum">The collector type to match against.</param>
        /// <returns>The full name of the matching collector, or null if no match is found.</returns>
        public string TryGetCollectorForPath(string name, CollectorTypeEnum collectorTypeEnum)
        {
            // Iterate through the registered searchers
            foreach (var searcher in searcherIndex)
            {
                // Ensure the collector type matches
                if (!collectorTypeEnum.HasFlag(searcher.Value.CollectorSearch.CollectorType))
                {
                    continue;
                }

                // Check if the path matches their search patterns.
                var folderSearchRegex = searcher.Value.FolderSearchRegex;
                var fileSearchRegex = searcher.Value.FileSearchRegex;

                bool folderMatch = false;
                bool fileMatch = false;

                if (folderSearchRegex != null)
                {
                    if (folderSearchRegex.IsMatch(name))
                    {
                        Console.WriteLine($"Match for folder {name}");
                        folderMatch = true;
                    }
                }

                if (fileSearchRegex != null)
                {
                    string nameComponent = folderSearchRegex == null ? name : Path.GetFileName(name);
                    if (fileSearchRegex.IsMatch(nameComponent))
                    {
                        Console.WriteLine($"Match for file {name}");
                        fileMatch = true;
                    }
                }

                if ((folderSearchRegex == null || folderMatch) && (fileSearchRegex == null || fileMatch))
                {
                    return searcher.Key;
                }
            }

            return null;
        }

        /// <summary>
        /// Converts a directory wildcard search pattern to a regular expression.
        /// </summary>
        /// <param name="wildcardSearch">The wildcard search pattern (e.g., "C:\Data\*").</param>
        /// <param name="onlyHasFolderSearch">Indicates if only a folder search is present (affects regex ending).</param>
        /// <returns>The compiled <see cref="Regex"/> or null if the pattern is empty.</returns>
        private static Regex ConvertDirectoryWildcardSearchToRegex(string wildcardSearch, bool onlyHasFolderSearch)
        {
            if (string.IsNullOrEmpty(wildcardSearch))
                return null;

            var regexSearch = wildcardSearch
                .Replace(@"\", @"\\")
                .Replace(".", @"\.")
                .Replace("*", "[^\\/]*")
                .Replace("?", ".") + (onlyHasFolderSearch ? "$" : "");
            return new Regex(regexSearch, RegexOptions.IgnoreCase);
        }

        /// <summary>
        /// Converts a file wildcard search pattern to a regular expression.
        /// </summary>
        /// <param name="wildcardSearch">The wildcard search pattern (e.g., "*.txt").</param>
        /// <returns>The compiled <see cref="Regex"/> or null if the pattern is empty.</returns>
        private static Regex ConvertFileWildcardSearchToRegex(string wildcardSearch)
        {
            if (string.IsNullOrEmpty(wildcardSearch))
                return null;

            var regexSearch = wildcardSearch
                .Replace(@"\", @"\\")
                .Replace(".", @"\.")
                .Replace("*", ".*")
                .Replace("?", ".") + "$";
            return new Regex(regexSearch, RegexOptions.IgnoreCase);
        }
    }
}
