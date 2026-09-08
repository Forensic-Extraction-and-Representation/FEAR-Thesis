using PropertyType = System.String;
using PropertyName = System.String;

namespace FEAR.Domain.KnowledgeGraph
{
    /// <summary>
    /// Represents a set of properties for an entity in the knowledge graph.
    /// Manages property values grouped by property name and provides methods for querying and manipulating these properties.
    /// </summary>
    public class PropertySet
    {
        // Internal dictionary mapping property names to lists of property values.
        private Dictionary<PropertyName, PropertyValueList> _properties = new Dictionary<PropertyName, PropertyValueList>();

        /// <summary>
        /// Adds a property value to the set under the specified property name.
        /// If the property already exists, the value is appended; otherwise, a new entry is created.
        /// </summary>
        /// <param name="propertyName">The name of the property.</param>
        /// <param name="propertyValue">The value to add.</param>
        public void AddProperty(PropertyName propertyName, PropertyValue propertyValue)
        {
            // Add the property value to the dictionary under the specified property name.
            if (_properties.ContainsKey(propertyName))
            {
                _properties[propertyName].Add(propertyValue);
            }
            else
            {
                // If the property name does not exist, create a new PropertyValueList and add the property value to it.
                var pl = new PropertyValueList() { PropertyName = propertyName };
                pl.Add(propertyValue);
                _properties.Add(propertyName, pl);
            }
        }

        /// <summary>
        /// Returns all property values in the set, across all property names.
        /// </summary>
        /// <returns>An enumerable of all <see cref="PropertyValue"/> objects.</returns>
        public IEnumerable<PropertyValue> AllProperties()
        {
            List<PropertyValue> output = new List<PropertyValue>();
            foreach (var property in _properties)
            {
                output.AddRange(property.Value.ObjectSet);
            }

            return output;
        }

        /// <summary>
        /// Gets all property values for a given property name.
        /// </summary>
        /// <param name="propertyName">The property name to look up.</param>
        /// <returns>A list of <see cref="PropertyValue"/> objects for the specified property name.</returns>
        internal IList<PropertyValue> GetPropertyValuesByName(string propertyName)
        {
            if (_properties.ContainsKey(propertyName))
                return _properties[propertyName].ObjectSet;
            else
                return new List<PropertyValue>();
        }

        /// <summary>
        /// Determines whether the set contains any values for the specified property name.
        /// </summary>
        /// <param name="propertyName">The property name to check.</param>
        /// <returns>True if the property exists and has values; otherwise, false.</returns>
        internal bool HasPropertyValue(string propertyName)
        {
            return _properties.ContainsKey(propertyName) && _properties[propertyName].ObjectSet.Count > 0;
        }

        /// <summary>
        /// Determines whether the set contains any properties.
        /// </summary>
        /// <returns>True if there are any properties; otherwise, false.</returns>
        internal bool HasProperties()
        {
            return _properties.Count > 0;
        }
    }
}
