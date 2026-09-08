    using PropertyType = System.String;

namespace FEAR.Domain.KnowledgeGraph
{
    /// <summary>
    /// Represents a property value within the knowledge graph.
    /// Encapsulates the property's type, its value, and the property name.
    /// </summary>
    public class PropertyValue
    {
        /// <summary>
        /// Gets or sets the type of the property (e.g., "Entity", "string", "int").
        /// </summary>
        public PropertyType PropertyType { get; set; }

        /// <summary>
        /// Gets or sets the value of the property.
        /// </summary>
        public object Value { get; set; }

        /// <summary>
        /// Gets or sets the name of the property.
        /// </summary>
        public string PropertyName { get; set; }
    }
}
