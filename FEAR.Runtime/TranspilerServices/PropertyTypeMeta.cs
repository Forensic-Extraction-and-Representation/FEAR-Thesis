namespace FEAR.Runtime.TranspilerServices
{
    /// <summary>
    /// Represents metadata about a property of an entity, including its name, type, and relationship.
    /// </summary>
    public class PropertyTypeMeta
    {
        /// <summary>
        /// The name of the property.
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// The type of the property (e.g., "string", "int", or a custom type).
        /// </summary>
        public string Type { get; set; }
        /// <summary>
        /// An ephemeral (temporary or generated) name for the property, if needed.
        /// </summary>
        public string EphemeralName { get; set; }
        /// <summary>
        /// The relationship type of the property (data, object, or collection).
        /// </summary>
        public RelationType RelationType { get; internal set; } = RelationType.DataType;

        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyTypeMeta"/> class.
        /// </summary>
        /// <param name="name">The property name.</param>
        /// <param name="type">The property type.</param>
        public PropertyTypeMeta(string name, string type)
        {
            Name = name;
            Type = type;
        }
    }
}
