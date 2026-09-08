using FEAR.Domain.KnowledgeGraph.Graphs;

namespace FEAR.Domain.KnowledgeGraph.EntitySearch
{
    /// <summary>
    /// Defines a factory for creating <see cref="IFindEntityStrategy"/> instances
    /// based on the provided <see cref="IRealGraph"/> implementation.
    /// Implementations encapsulate the logic for selecting the appropriate strategy
    /// for searching or resolving entities in different types of graphs.
    /// </summary>
    public interface IFindEntityStrategyFactory
    {
        /// <summary>
        /// Creates an <see cref="IFindEntityStrategy"/> for the specified graph.
        /// </summary>
        /// <param name="graph">The graph for which to create the entity search strategy.</param>
        /// <returns>An <see cref="IFindEntityStrategy"/> suitable for the given graph.</returns>
        IFindEntityStrategy CreateStrategy(IRealGraph graph);
    }
}
