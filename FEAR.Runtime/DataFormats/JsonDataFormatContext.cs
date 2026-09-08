using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Dynamic;
using System.Text.Json;

namespace FEAR.Runtime.DataFormats
{
    /// <summary>
    /// Provides a data format context for handling JSON files and content.
    /// Implements <see cref="IDataFormatContext"/> to support reading JSON data from files or strings.
    /// </summary>
    public class JsonDataFormatContext : IDataFormatContext
    {
        /// <summary>
        /// Represents the state for a JSON data context, providing access to parsed JSON records.
        /// </summary>
        public class JsonDataFormatContextState : IDataFormatContextState
        {
            /// <summary>
            /// Gets an enumerable of dynamic records parsed from the JSON source.
            /// If the root is an array, yields each element as a dynamic object.
            /// If the root is an object, yields a single dynamic object.
            /// </summary>
            public IEnumerable<dynamic> Data
            {
                get
                {
                    ExpandoObjectConverter converter = new ExpandoObjectConverter();
                    if (jsonDocument.RootElement.ValueKind == JsonValueKind.Array)
                    {
                        // If the root is an array, enumerate each element
                        foreach (var element in jsonDocument.RootElement.EnumerateArray())
                        {
                            dynamic data = JsonConvert.DeserializeObject<ExpandoObject>(element.GetRawText(), converter);
                            yield return data;
                        }
                    }
                    else
                    {
                        // If the root is an object, yield the single object
                        dynamic data = JsonConvert.DeserializeObject<ExpandoObject>(jsonDocument.RootElement.GetRawText(), converter);
                        yield return data;
                    }
                }
            }

            private JsonDocument jsonDocument;

            private JsonDataFormatContextState() { }

            /// <summary>
            /// Creates a new state from a JSON file.
            /// </summary>
            /// <param name="file">The path to the JSON file.</param>
            /// <returns>A new <see cref="JsonDataFormatContextState"/> for the file.</returns>
            public static JsonDataFormatContextState FromFile(string file)
            {
                var jdfc = new JsonDataFormatContextState()
                {
                    jsonDocument = JsonDocument.Parse(File.ReadAllText(file))
                };

                return jdfc;
            }

            /// <summary>
            /// Creates a new state from JSON content in a string.
            /// </summary>
            /// <param name="content">The JSON content as a string.</param>
            /// <returns>A new <see cref="JsonDataFormatContextState"/> for the content.</returns>
            public static JsonDataFormatContextState FromContent(string content)
            {
                var jdfc = new JsonDataFormatContextState()
                {
                    jsonDocument = JsonDocument.Parse(content)
                };

                return jdfc;
            }
        }

        /// <summary>
        /// Gets the MIME type for JSON content.
        /// </summary>
        public string ContentMimeType => "application/json";

        /// <summary>
        /// Gets the supported format identifiers for JSON.
        /// </summary>
        public string[] Format => new string[] { "JSON", "application/json" };

        /// <summary>
        /// Gets the file extension filter for JSON files.
        /// </summary>
        public string[] FileExtensionFilter => new[] { "*.json" };

        /// <summary>
        /// Creates a new state from a JSON file.
        /// </summary>
        /// <param name="file">The path to the JSON file.</param>
        /// <returns>A new <see cref="IDataFormatContextState"/> for the file.</returns>
        public IDataFormatContextState CreateStateFromFile(string file)
        {
            return JsonDataFormatContextState.FromFile(file);
        }

        /// <summary>
        /// Creates a new state from JSON content in a string.
        /// </summary>
        /// <param name="content">The JSON content as a string.</param>
        /// <returns>A new <see cref="IDataFormatContextState"/> for the content.</returns>
        public IDataFormatContextState CreateStateFromContent(string content)
        {
            return JsonDataFormatContextState.FromContent(content);
        }
    }
}
