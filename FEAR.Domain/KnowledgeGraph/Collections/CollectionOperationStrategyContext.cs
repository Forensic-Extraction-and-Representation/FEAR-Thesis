using FEAR.Domain.KnowledgeGraph.Graphs;
using VDS.RDF;

namespace FEAR.Domain.KnowledgeGraph.Collections
{
    /// <summary>
    /// Encapsulates all contextual information required to perform a collection operation
    /// (such as add, update, or remove) on an RDF graph.
    /// </summary>
    public class CollectionOperationStrategyContext
    {
        /// <summary>
        /// The RDF graph where the collection operation will be performed.
        /// </summary>
        public IRealGraph Graph { get; set; }

        /// <summary>
        /// The collection property being operated on (e.g., Bag, List), including its type and elements.
        /// </summary>
        public CollectionProperty CollectionProperty { get; set; }

        /// <summary>
        /// The target entity (subject) in the graph for which the collection property is being manipulated.
        /// </summary>
        public Entity TargetEntity { get; set; }

        /// <summary>
        /// The RDF property node (predicate) representing the collection property in the graph.
        /// </summary>
        public INode PropertyUriNode { get; set; }

        /// <summary>
        /// The blank node representing the head of an RDF list, if applicable.
        /// Null if the collection is not a list.
        /// </summary>
        public IBlankNode? ListNode { get; set; }

        /// <summary>
        /// The name or identifier of the graph where the operation is targeted.
        /// </summary>
        public string GraphName { get; set; }
    }
}
