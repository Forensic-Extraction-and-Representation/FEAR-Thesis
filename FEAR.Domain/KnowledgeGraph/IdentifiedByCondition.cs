namespace FEAR.Domain.KnowledgeGraph
{
    /// <summary>
    /// Specifies the identification requirement for a property or entity in the knowledge graph.
    /// Used to indicate whether a property or entity is required, optional, or not used for identification.
    /// </summary>
    public enum IdentifiedByCondition
    {
        /// <summary>
        /// The property or entity is required for identification.
        /// </summary>
        Required,

        /// <summary>
        /// The property or entity is optional for identification.
        /// </summary>
        Optional,

        /// <summary>
        /// The property or entity is not used for identification.
        /// </summary>
        None
    }
}
