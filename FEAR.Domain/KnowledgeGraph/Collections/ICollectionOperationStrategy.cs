namespace FEAR.Domain.KnowledgeGraph.Collections
{
    /// <summary>
    /// Defines a strategy interface for performing operations on RDF collections
    /// (such as Bag or List) within a knowledge graph context.
    /// </summary>
    public interface ICollectionOperationStrategy
    {
        /// <summary>
        /// Performs an operation on an RDF Bag collection using the provided context.
        /// </summary>
        /// <param name="cosc">The context containing all necessary information for the operation.</param>
        /// <returns>A result object containing triples to assert/retract and any queries to execute.</returns>
        CollectionOperationResult BagOperation(CollectionOperationStrategyContext cosc);

        /// <summary>
        /// Performs an operation on an RDF List collection using the provided context.
        /// </summary>
        /// <param name="cosc">The context containing all necessary information for the operation.</param>
        /// <returns>A result object containing triples to assert/retract and any queries to execute.</returns>
        CollectionOperationResult ListOperation(CollectionOperationStrategyContext cosc);
    }
}
