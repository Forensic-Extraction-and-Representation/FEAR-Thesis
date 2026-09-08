using System.Text.RegularExpressions;

namespace FEAR.Runtime.DataFormats
{
    /// <summary>
    /// Factory for resolving and instantiating <see cref="IDataFormatContext"/> implementations
    /// based on format identifiers or MIME types. Scans all loaded assemblies for types implementing
    /// <see cref="IDataFormatContext"/> (excluding <see cref="NoDataFormatContext"/>), and maps
    /// their supported formats for quick lookup.
    /// </summary>
    public class DataFormatContextFactory
    {
        private Lazy<IDictionary<string, IDataFormatContext>> _dataFormats = null;

        /// <summary>
        /// Initializes a new instance of the <see cref="DataFormatContextFactory"/> class.
        /// Scans all loaded assemblies for <see cref="IDataFormatContext"/> implementations and builds a lookup dictionary.
        /// </summary>
        public DataFormatContextFactory()
        {
            _dataFormats = new Lazy<IDictionary<string, IDataFormatContext>>(() =>
            {
                // Find all types that implement IDataFormatContext (excluding NoDataFormatContext)
                var types = AppDomain.CurrentDomain.GetAssemblies()
                    .SelectMany(s => s.GetTypes())
                    .Where(p => typeof(IDataFormatContext).IsAssignableFrom(p) && !p.IsInterface)
                    .Except(new[] { typeof(NoDataFormatContext) });

                // Instantiate each context and map all its supported formats (case-insensitive) to the instance
                return types.Select(t => (IDataFormatContext)Activator.CreateInstance(t))
                    .SelectMany(x => x.Format.Select(f => new { Format = f, Context = x }))
                    .ToDictionary(x => x.Format.ToLower(), x => x.Context);
            });
        }

        // Regex to extract the main content type (e.g., "text/csv" from "text/csv; charset=utf-8")
        Regex contentType = new Regex(@"([^;]*)");

        /// <summary>
        /// Resolves and returns an <see cref="IDataFormatContext"/> for the given format identifier or MIME type.
        /// If no match is found, returns a <see cref="NoDataFormatContext"/> instance.
        /// </summary>
        /// <param name="format">The format identifier or MIME type (e.g., "csv", "text/csv").</param>
        /// <returns>An <see cref="IDataFormatContext"/> instance for the format, or a fallback if not found.</returns>
        public IDataFormatContext Create(string format)
        {
            format = contentType.Matches(format.ToLower()).FirstOrDefault()?.Value;
            if (_dataFormats.Value.ContainsKey(format))
                return _dataFormats.Value[format];

            return new NoDataFormatContext();
        }
    }
}
