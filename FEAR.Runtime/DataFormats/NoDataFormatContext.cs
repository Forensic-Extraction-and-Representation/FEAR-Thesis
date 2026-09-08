namespace FEAR.Runtime.DataFormats
{
    /// <summary>
    /// Represents a fallback or "no data" format context for unsupported or unknown data formats.
    /// Implements <see cref="IDataFormatContext"/> and always returns an empty state.
    /// Used when no other data format context matches the requested format.
    /// </summary>
    public class NoDataFormatContext : IDataFormatContext
    {
        /// <summary>
        /// Represents an empty state for the no data format context.
        /// </summary>
        public class NoDataFormatContextState : IDataFormatContextState
        {
            /// <summary>
            /// Gets an empty enumerable, as no data is available for unsupported formats.
            /// </summary>
            public IEnumerable<dynamic> Data => new List<dynamic>();
        }

        /// <summary>
        /// Gets the MIME type for the no data format context (default is "text/html").
        /// </summary>
        public string ContentMimeType => "text/html";

        /// <summary>
        /// Gets the supported format identifiers (empty for no data format).
        /// </summary>
        public string[] Format => new[] { "" };

        /// <summary>
        /// Gets the file extension filters (empty for no data format).
        /// </summary>
        public string[] FileExtensionFilter => new[] { "" };

        /// <summary>
        /// Returns an empty state for any file input.
        /// </summary>
        /// <param name="file">The file path (ignored).</param>
        /// <returns>An empty <see cref="IDataFormatContextState"/>.</returns>
        public IDataFormatContextState CreateStateFromFile(string file)
        {
            return new NoDataFormatContextState();
        }

        /// <summary>
        /// Returns an empty state for any content input.
        /// </summary>
        /// <param name="content">The content string (ignored).</param>
        /// <returns>An empty <see cref="IDataFormatContextState"/>.</returns>
        public IDataFormatContextState CreateStateFromContent(string content)
        {
            return new NoDataFormatContextState();
        }
    }
}
