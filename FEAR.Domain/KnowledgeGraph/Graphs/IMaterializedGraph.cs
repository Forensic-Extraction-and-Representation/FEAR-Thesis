using FEAR.Domain.KnowledgeGraph.Collections;
using FEAR.Domain.KnowledgeGraph.GraphUpdate;

namespace FEAR.Domain.KnowledgeGraph.Graphs
{
    /// <summary>
    /// Represents a materialized and complete graph that should be persisted.
    /// Inherits from <see cref="IRealGraph"/> and adds methods for graph
    /// manipulation and state management.
    /// </summary>
    public interface IMaterializedGraph : IRealGraph
    {
        /// <summary>
        /// Gets or sets a value indicating whether triple assertions and retractions should be tracked.
        /// When enabled, changes to the graph are recorded for auditing or synchronization purposes.
        /// </summary>
        bool TrackTriples { get; set; }

        /// <summary>
        /// Adds or updates properties on the specified entity within the graph, using the provided context and property set.
        /// </summary>
        /// <param name="entityContext">The context for the graph update operation.</param>
        /// <param name="targetEntity">The entity to update.</param>
        /// <param name="properties">The set of properties to add or update.</param>
        void AddOrUpdateProperties(GraphUpdateContext entityContext, Entity targetEntity, PropertySet properties);

        /// <summary>
        /// Removes all triples and entities from the graph, resetting its state.
        /// </summary>
        void Clear();

        /// <summary>
        /// Returns the total number of triples currently present in the graph.
        /// </summary>
        /// <returns>The count of triples.</returns>
        int TriplesCount();

        /// <summary>
        /// Executes the result of a collection operation, such as adding or removing triples or running a SPARQL query.
        /// </summary>
        /// <param name="cor">The result of the collection operation to execute.</param>
        void ExecuteCollectionResult(CollectionOperationResult cor);

        /// <summary>
        /// Gets statistics about the graph, such as the number of entities, triples, and other relevant metrics.
        /// </summary>
        /// <returns>A <see cref="GraphStatistics"/> object containing the graph's statistics.</returns>
        GraphStatistics GetStatistics();
    }
}
