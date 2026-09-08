using CsvHelper;
using System.Globalization;

namespace FEAR.Runtime.DataFormats
{
    /// <summary>
    /// Provides a data format context for handling CSV files and content.
    /// Implements <see cref="IDataFormatContext"/> to support reading CSV data from files or strings.
    /// </summary>
    public class CsvDataFormatContext : IDataFormatContext
    {
        /// <summary>
        /// Represents the state for a CSV data context, providing access to parsed CSV records.
        /// </summary>
        public class CsvDataFormatContextState : IDataFormatContextState
        {
            /// <summary>
            /// Gets an enumerable of dynamic records parsed from the CSV source.
            /// </summary>
            public IEnumerable<dynamic> Data
            {
                get
                {
                    using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
                    {
                        var records = csv.GetRecords<dynamic>();
                        foreach (var record in records)
                            yield return record;
                    }
                }
            }

            private StreamReader reader;

            private CsvDataFormatContextState() { }

            /// <summary>
            /// Creates a new state from a CSV file.
            /// </summary>
            /// <param name="file">The path to the CSV file.</param>
            /// <returns>A new <see cref="CsvDataFormatContextState"/> for the file.</returns>
            public static CsvDataFormatContextState FromFile(string file)
            {
                return new CsvDataFormatContextState()
                {
                    reader = new StreamReader(file)
                };
            }

            /// <summary>
            /// Creates a new state from CSV content in a string.
            /// </summary>
            /// <param name="content">The CSV content as a string.</param>
            /// <returns>A new <see cref="CsvDataFormatContextState"/> for the content.</returns>
            public static CsvDataFormatContextState FromContent(string content)
            {
                // Use a MemoryStream to wrap the string content for StreamReader
                var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(content));
                return new CsvDataFormatContextState()
                {
                    reader = new StreamReader(stream)
                };
            }
        }

        /// <summary>
        /// Gets the MIME type for CSV content.
        /// </summary>
        public string ContentMimeType => "text/csv";

        /// <summary>
        /// Gets the supported format identifiers for CSV.
        /// </summary>
        public string[] Format => new[] { "CSV", "text/csv" };

        /// <summary>
        /// Gets the file extension filter for CSV files.
        /// </summary>
        public string[] FileExtensionFilter => new[] { "*.csv" };

        /// <summary>
        /// Creates a new state from a CSV file.
        /// </summary>
        /// <param name="file">The path to the CSV file.</param>
        /// <returns>A new <see cref="IDataFormatContextState"/> for the file.</returns>
        public IDataFormatContextState CreateStateFromFile(string file)
        {
            return CsvDataFormatContextState.FromFile(file);
        }

        /// <summary>
        /// Creates a new state from CSV content in a string.
        /// </summary>
        /// <param name="content">The CSV content as a string.</param>
        /// <returns>A new <see cref="IDataFormatContextState"/> for the content.</returns>
        public IDataFormatContextState CreateStateFromContent(string content)
        {
            return CsvDataFormatContextState.FromContent(content);
        }
    }
}
