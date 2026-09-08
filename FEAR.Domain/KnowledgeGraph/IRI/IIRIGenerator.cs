namespace FEAR.Domain.KnowledgeGraph.IRI
{
    /// <summary>
    /// Defines a contract for generating IRIs (Internationalized Resource Identifiers) for entities in the knowledge graph.
    /// Implementations are responsible for creating unique, context-specific IRIs and can optionally reset their internal state.
    /// </summary>
    public interface IIRIGenerator
    {
        /// <summary>
        /// Creates a new IRI for an entity, using the specified type and URI segment.
        /// </summary>
        /// <param name="type">The type of entity (used in the IRI if no segment is provided).</param>
        /// <param name="uriSegment">A custom segment for the IRI. If null or empty, a unique segment should be generated.</param>
        /// <returns>A new <see cref="Uri"/> representing the entity's IRI.</returns>
        Uri CreateIRI(string type, string uriSegment);

        /// <summary>
        /// Resets the generator state. Implementations can use this to clear or reinitialize any internal state if needed.
        /// </summary>
        void Reset();
    }
}