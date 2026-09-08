using FEAR.Domain.KnowledgeGraph.GraphUpdate;

namespace FEAR.Domain.KnowledgeGraph.EntitySearch
{
    /// <summary>
    /// Defines a strategy for finding an entity within a knowledge graph update context.
    /// Implementations encapsulate the logic for locating or resolving an <see cref="Entity"/>
    /// based on the provided <see cref="GraphUpdateContext"/>.
    /// </summary>
    public interface IFindEntityStrategy
    {
        /// <summary>
        /// Attempts to find or resolve an <see cref="Entity"/> using the specified update context.
        /// </summary>
        /// <param name="entityContext">The context containing graph, entity, and update information.</param>
        /// <returns>
        /// A <see cref="KGResponse{Entity}"/> containing the found entity if successful,
        /// or error details if the operation fails.
        /// </returns>
        KGResponse<Entity> FindEntity(GraphUpdateContext entityContext);
    }
}
