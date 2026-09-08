namespace FEAR.Domain.KnowledgeGraph
{
    /// <summary>
    /// Represents an identification option for an entity in the knowledge graph.
    /// Specifies how an entity is identified, either by a property or by another entity,
    /// and the condition under which this identification applies.
    /// </summary>
    public class IdentifiedByOption
    {
        /// <summary>
        /// Gets or sets the identification condition (e.g., required, optional, none).
        /// </summary>
        public IdentifiedByCondition IdentifiedByCondition { get; set; }

        /// <summary>
        /// Gets or sets the type of identification (by property or by entity).
        /// </summary>
        public IdentifiedByType IdentifiedByType { get; set; }

        /// <summary>
        /// Gets or sets the entity used for identification, if applicable.
        /// Used when identification is based on a related entity.
        /// </summary>
        public Entity Entity { get; set; }

        /// <summary>
        /// Gets or sets the property name used for identification, if applicable.
        /// Used when identification is based on a property value.
        /// </summary>
        public string Property { get; set; }
    }
}
