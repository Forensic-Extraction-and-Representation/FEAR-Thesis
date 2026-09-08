namespace FEAR.Domain.KnowledgeGraph
{
    /// <summary>
    /// Specifies the method by which an entity is identified in the knowledge graph.
    /// Used to distinguish between identification by a related entity or by a property value.
    /// </summary>
    public enum IdentifiedByType
    {
        /// <summary>
        /// The entity is identified by another related entity.
        /// </summary>
        Entity,

        /// <summary>
        /// The entity is identified by a property value.
        /// </summary>
        Property
    }
}
