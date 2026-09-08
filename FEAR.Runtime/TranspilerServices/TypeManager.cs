namespace FEAR.Runtime.TranspilerServices
{
    /// <summary>
    /// Manages entity types, their properties, and namespace prefixes for the transpiler.
    /// </summary>
    public class TypeManager
    {
        /// <summary>
        /// Stores namespace prefixes and their corresponding URLs.
        /// </summary>
        public Dictionary<string, string> Prefixes { get; set; } = new Dictionary<string, string>();
        /// <summary>
        /// Stores entity types and their metadata, keyed by entity name.
        /// </summary>
        public Dictionary<string, EntityTypeMeta> EntityTypes = new Dictionary<string, EntityTypeMeta>();

        public TypeManager() { }

        /// <summary>
        /// Adds a new entity type if it does not already exist.
        /// </summary>
        /// <param name="entityName">The name of the entity.</param>
        /// <param name="typeName">The type of the entity.</param>
        /// <returns>The <see cref="EntityTypeMeta"/> for the entity.</returns>
        public EntityTypeMeta AddEntityType(string entityName, string typeName)
        {
            EntityTypeMeta etm = null;
            if (!EntityTypes.TryGetValue(entityName, out etm))
            {
                etm = new EntityTypeMeta(entityName, typeName);
                EntityTypes.Add(entityName, etm);
            }

            return etm;
        }

        /// <summary>
        /// Adds a property to an existing entity type.
        /// </summary>
        /// <param name="entityName">The name of the entity.</param>
        /// <param name="property">The property name.</param>
        /// <param name="typeMeta">The property metadata.</param>
        public void AddProperty(string entityName, string property, PropertyTypeMeta typeMeta)
        {
            if (EntityTypes.ContainsKey(entityName))
                EntityTypes[entityName].AddProperty(property, typeMeta);
        }

        /// <summary>
        /// Gets the metadata for a specified entity type.
        /// </summary>
        /// <param name="entityName">The name of the entity.</param>
        /// <returns>The <see cref="EntityTypeMeta"/> for the entity, or null if not found.</returns>
        public EntityTypeMeta GetEntityType(string entityName)
        {
            EntityTypeMeta? entityTypeMeta;
            EntityTypes.TryGetValue(entityName, out entityTypeMeta);

            return entityTypeMeta;
        }

        /// <summary>
        /// Adds a namespace prefix and its URL if it does not already exist.
        /// </summary>
        /// <param name="prefix">The prefix to add.</param>
        /// <param name="url">The corresponding URL.</param>
        public void AddPrefix(string prefix, string url)
        {
            if(!Prefixes.ContainsKey(prefix))
                Prefixes.Add(prefix, url);
        }
    }
}
