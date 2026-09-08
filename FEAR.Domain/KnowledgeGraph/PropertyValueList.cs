using PropertyName = System.String;

namespace FEAR.Domain.KnowledgeGraph
{
    /// <summary>
    /// Represents a list of property values associated with a specific property name in the knowledge graph.
    /// Provides methods to add property values and filter them by type.
    /// </summary>
    public class PropertyValueList
    {
        // Internal list of property values for this property name.
        private List<PropertyValue> _objectSet = new List<PropertyValue>();

        /// <summary>
        /// Gets or sets the property name associated with this list of values.
        /// </summary>
        public PropertyName PropertyName { get; set; }

        /// <summary>
        /// Gets the list of property values for this property name.
        /// </summary>
        public List<PropertyValue> ObjectSet => _objectSet;

        /// <summary>
        /// Adds a property value to the list.
        /// </summary>
        /// <param name="propertyValue">The property value to add.</param>
        public void Add(PropertyValue propertyValue)
        {
            _objectSet.Add(propertyValue);
        }

        /// <summary>
        /// Returns all property values in the list that match the specified property type.
        /// </summary>
        /// <param name="propertyType">The property type to filter by (e.g., "Entity", "string").</param>
        /// <returns>An enumerable of matching <see cref="PropertyValue"/> objects.</returns>
        public IEnumerable<PropertyValue> GetPropertiesOfType(string propertyType)
        {
            return _objectSet.Where(t => t.PropertyType == propertyType);
        }
    }
}