using System.Dynamic;
using System.Xml;
using System.Xml.Linq;

namespace FEAR.Runtime.DataFormats
{
    /// <summary>
    /// Provides a data format context for handling XML files and content.
    /// Implements <see cref="IDataFormatContext"/> to support reading XML data from files or strings.
    /// </summary>
    public class XmlDataFormatContext : IDataFormatContext
    {
        /// <summary>
        /// Represents the state for an XML data context, providing access to parsed XML records as dynamic objects.
        /// </summary>
        public class XmlDataFormatContextState : IDataFormatContextState
        {
            private XDocument xmlDocument;

            private XmlDataFormatContextState() {
                Console.WriteLine("#################### Untested code within this class. ####################");
            }

            /// <summary>
            /// Gets an enumerable of dynamic records parsed from the XML source.
            /// If the root contains multiple child elements, yields each as a dynamic object.
            /// If the root contains a single element, yields that as a dynamic object.
            /// </summary>
            public IEnumerable<dynamic> Data
            {
                get
                {
                    // If the root has multiple child elements, yield each as a dynamic object
                    foreach (var element in xmlDocument.Root.Elements())
                    {
                        yield return XElementToDynamic(element);
                    }
                }
            }

            /// <summary>
            /// Creates a new state from an XML file.
            /// </summary>
            /// <param name="file">The path to the XML file.</param>
            /// <returns>A new <see cref="XmlDataFormatContextState"/> for the file.</returns>
            public static XmlDataFormatContextState FromFile(string file)
            {
                return new XmlDataFormatContextState
                {
                    xmlDocument = XDocument.Load(file)
                };
            }

            /// <summary>
            /// Creates a new state from XML content in a string.
            /// </summary>
            /// <param name="content">The XML content as a string.</param>
            /// <returns>A new <see cref="XmlDataFormatContextState"/> for the content.</returns>
            public static XmlDataFormatContextState FromContent(string content)
            {
                return new XmlDataFormatContextState
                {
                    xmlDocument = XDocument.Parse(content)
                };
            }

            /// <summary>
            /// Recursively converts an XElement to a dynamic ExpandoObject.
            /// </summary>
            private static dynamic XElementToDynamic(XElement element)
            {
                IDictionary<string, object> expando = new ExpandoObject();

                // Add attributes as properties
                foreach (var attr in element.Attributes())
                {
                    expando[attr.Name.LocalName] = attr.Value;
                }

                // Add child elements
                var grouped = element.Elements().GroupBy(e => e.Name.LocalName);
                foreach (var group in grouped)
                {
                    var groupList = group.Select(XElementToDynamic).ToList();
                    expando[group.Key] = groupList.Count == 1 ? groupList[0] : groupList;
                }

                // If element has no child elements, set its value
                if (!element.HasElements && !string.IsNullOrWhiteSpace(element.Value))
                {
                    expando["Value"] = element.Value;
                }

                return expando;
            }
        }

        /// <summary>
        /// Gets the MIME type for XML content.
        /// </summary>
        public string ContentMimeType => "text/xml";

        /// <summary>
        /// Gets the supported format identifiers for XML.
        /// </summary>
        public string[] Format => new[] { "XML", "text/xml" };

        /// <summary>
        /// Gets the file extension filter for XML files.
        /// </summary>
        public string[] FileExtensionFilter => new[] { ".xml" };

        /// <summary>
        /// Creates a new state from an XML file.
        /// </summary>
        /// <param name="file">The path to the XML file.</param>
        /// <returns>A new <see cref="IDataFormatContextState"/> for the file.</returns>
        public IDataFormatContextState CreateStateFromFile(string file)
        {
            return XmlDataFormatContextState.FromFile(file);
        }

        /// <summary>
        /// Creates a new state from XML content in a string.
        /// </summary>
        /// <param name="content">The XML content as a string.</param>
        /// <returns>A new <see cref="IDataFormatContextState"/> for the content.</returns>
        public IDataFormatContextState CreateStateFromContent(string content)
        {
            return XmlDataFormatContextState.FromContent(content);
        }
    }
}
