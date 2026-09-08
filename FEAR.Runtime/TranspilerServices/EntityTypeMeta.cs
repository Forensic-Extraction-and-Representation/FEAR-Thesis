namespace FEAR.Runtime.TranspilerServices
{
    /// <summary>
    /// Represents metadata about an entity type, including its properties and additional types.
    /// </summary>
    public class EntityTypeMeta
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EntityTypeMeta"/> class.
        /// </summary>
        /// <param name="name">The entity name.</param>
        /// <param name="type">The entity type.</param>
        public EntityTypeMeta(string name, string type)
        {
            Name = name;
            Type = type;
            EphemeralName = Name + "__ephemeral";
        }

        /// <summary>
        /// An ephemeral (temporary or generated) name for the entity.
        /// </summary>
        public string EphemeralName { get; set; }
        /// <summary>
        /// The name of the entity.
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// The main type of the entity.
        /// </summary>
        public string Type { get; set; }
        /// <summary>
        /// Additional types associated with the entity.
        /// </summary>
        public List<string> ExtraTypes { get; set; } = new List<string>();
        /// <summary>
        /// The properties of the entity, keyed by property name.
        /// </summary>
        public Dictionary<string, PropertyTypeMeta> PropertyTypes { get; set; } = new Dictionary<string, PropertyTypeMeta>();

        /// <summary>
        /// Adds an additional type to the entity.
        /// </summary>
        /// <param name="type">The type to add.</param>
        public void AddType(string type)
        {
            if (!ExtraTypes.Contains(type))
                ExtraTypes.Add(type);
        }

        /// <summary>
        /// Adds a property to the entity.
        /// </summary>
        /// <param name="name">The property name.</param>
        /// <param name="typeMeta">The property metadata.</param>
        public void AddProperty(string name, PropertyTypeMeta typeMeta)
        {
            if (!PropertyTypes.ContainsKey(name))
                PropertyTypes.Add(name, typeMeta);
        }
    }
}
