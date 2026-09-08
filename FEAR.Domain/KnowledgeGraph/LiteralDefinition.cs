using FEAR.Domain.Helpers;

namespace FEAR.Domain.KnowledgeGraph
{
    /// <summary>
    /// Represents a literal property definition for use in the knowledge graph.
    /// Encapsulates the property URI, its value, and the XSD type URI for serialization or graph operations.
    /// </summary>
    public class LiteralDefinition
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LiteralDefinition"/> class with a property URI, value, and type URI.
        /// </summary>
        /// <param name="property">The URI of the property.</param>
        /// <param name="value">The value of the property.</param>
        /// <param name="typeUri">The XSD type URI of the value.</param>
        public LiteralDefinition(string property, object value, string typeUri)
        {
            PropertyUri = property;
            PropertyValue = value.ToString();
            TypeUri = typeUri;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LiteralDefinition"/> class with a property URI, value, and .NET type.
        /// The type is converted to an XSD type URI using <see cref="TypeUriHelpers.GetTypeUriOfType(Type)"/>.
        /// </summary>
        /// <param name="property">The URI of the property.</param>
        /// <param name="value">The value of the property.</param>
        /// <param name="type">The .NET type of the value, which is used to determine the XSD type URI.</param>
        public LiteralDefinition(string property, object value, Type type) : this(property, value, TypeUriHelpers.GetTypeUriOfType(type))
        {
        }

        /// <summary>
        /// Gets or sets the URI of the property.
        /// </summary>
        public string PropertyUri { get; set; }

        /// <summary>
        /// Gets or sets the string value of the property.
        /// </summary>
        public string PropertyValue { get; set; }

        /// <summary>
        /// Gets or sets the XSD type URI of the value.
        /// </summary>
        public string TypeUri { get; set; }
    }
}