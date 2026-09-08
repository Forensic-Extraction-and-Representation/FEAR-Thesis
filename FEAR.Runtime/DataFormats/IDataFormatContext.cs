namespace FEAR.Runtime.DataFormats
{
    /// <summary>
    /// Defines the contract for a data format context, which provides information and state creation
    /// for handling specific data formats (such as file types, MIME types, and content parsing).
    /// Implementations are responsible for describing supported formats and producing state objects
    /// from files or content strings for further processing.
    /// </summary>
    public interface IDataFormatContext
    {
        /// <summary>
        /// Gets the MIME type associated with the data format (e.g., "application/json", "text/csv").
        /// </summary>
        string ContentMimeType { get; }

        /// <summary>
        /// Gets the list of format identifiers or names supported by this context (e.g., "json", "csv").
        /// This can also be MIME types that may be received as part of web requests.
        /// </summary>
        string[] Format { get; }

        /// <summary>
        /// Gets the list of file extension filters associated with this data format (e.g., ".json", ".csv").
        /// </summary>
        string[] FileExtensionFilter { get; }

        /// <summary>
        /// Creates a new <see cref="IDataFormatContextState"/> from the specified file.
        /// </summary>
        /// <param name="file">The file path to load and parse.</param>
        /// <returns>A state object representing the parsed data from the file.</returns>
        IDataFormatContextState CreateStateFromFile(string file);

        /// <summary>
        /// Creates a new <see cref="IDataFormatContextState"/> from the specified content string.
        /// </summary>
        /// <param name="content">The content string to parse.</param>
        /// <returns>A state object representing the parsed data from the content.</returns>
        IDataFormatContextState CreateStateFromContent(string content);
    }
}
