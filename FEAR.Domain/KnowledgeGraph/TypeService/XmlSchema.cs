namespace FEAR.Domain.KnowledgeGraph.TypeService
{
    /// <summary>
    /// Contains types and helpers for working with XML Schema datatypes.
    /// </summary>
    public class XmlSchema
    {
        /// <summary>
        /// Represents an XML Schema xsd:dateTime value.
        /// Provides formatting to the standard xsd:dateTime string representation (UTC, ISO 8601).
        /// </summary>
        public class xsdDateTime 
        {
            /// <summary>
            /// The underlying <see cref="DateTime"/> value.
            /// </summary>
            protected readonly DateTime dateTime;

            /// <summary>
            /// Initializes a new instance of the <see cref="xsdDateTime"/> class with the specified <see cref="DateTime"/>.
            /// </summary>
            /// <param name="dt">The <see cref="DateTime"/> value to wrap.</param>
            public xsdDateTime(DateTime dt) {
                dateTime = dt;
            }

            /// <summary>
            /// Returns the date and time in xsd:dateTime (ISO 8601, UTC) string format.
            /// </summary>
            /// <returns>A string representation of the date and time in UTC, formatted as "yyyy-MM-ddTHH:mm:ssZ".</returns>
            public override string ToString()
            {
                return dateTime.ToUniversalTime().ToString("u").Replace(" ", "T");
            }
        }
    }
}
