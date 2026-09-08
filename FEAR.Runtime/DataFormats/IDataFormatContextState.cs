namespace FEAR.Runtime.DataFormats
{
    /// <summary>
    /// Represents the state of a data format context, providing access to parsed or loaded data.
    /// Implementations expose the data as an enumerable of dynamic objects, allowing consumers
    /// to iterate over records, rows, or entities regardless of the underlying format (CSV, JSON, etc.).
    /// </summary>
    public interface IDataFormatContextState
    {
        /// <summary>
        /// Gets the enumerable collection of dynamic data items parsed or loaded from the source.
        /// </summary>
        IEnumerable<dynamic> Data { get; }
    }
}
