using FEAR.Domain.KnowledgeGraph.GraphManager;
using FEAR.Domain.KnowledgeGraph.Graphs;
using FEAR.Domain.KnowledgeGraph.GraphUpdate;
using VDS.RDF;
using VDS.RDF.Parsing;
using VDS.RDF.Query;

namespace FEAR.Domain.KnowledgeGraph.Collections
{
    /// <summary>
    /// Abstract base class for collection operation strategies.
    /// Provides common functionality and context for handling RDF collection operations (Bag, List).
    /// </summary>
    public abstract class BaseCollectionOperationStrategy : ICollectionOperationStrategy
    {
        /// <summary>
        /// Executes a Bag operation on the collection using the provided context.
        /// </summary>
        /// <param name="cosc">The operation context containing collection and graph information.</param>
        /// <returns>Result of the collection operation.</returns>
        public abstract CollectionOperationResult BagOperation(CollectionOperationStrategyContext cosc);

        /// <summary>
        /// Executes a List operation on the collection using the provided context.
        /// </summary>
        /// <param name="cosc">The operation context containing collection and graph information.</param>
        /// <returns>Result of the collection operation.</returns>
        public abstract CollectionOperationResult ListOperation(CollectionOperationStrategyContext cosc);

        /// <summary>
        /// Provides access to the graph manager for ontology and graph operations.
        /// </summary>
        protected IGraphManager GraphManager { get; set; }

        /// <summary>
        /// Provides access to the temporary working graph for updates.
        /// </summary>
        protected IMaterializedGraph WorkingGraph => GraphUpdateContext.TemporaryGraph;

        /// <summary>
        /// Context for the current graph update operation.
        /// </summary>
        protected GraphUpdateContext GraphUpdateContext { get; }

        // Lazy initialization of commonly used RDF URI nodes.
        Lazy<IUriNode> rdfAUriNode = null;
        Lazy<IUriNode> rdfNilNode = null;
        Lazy<IUriNode> rdfFirstNode = null;
        Lazy<IUriNode> rdfRestNode = null;

        /// <summary>
        /// Gets the RDF 'a' (type) URI node.
        /// </summary>
        protected IUriNode RdfAUriNode => rdfAUriNode.Value;

        /// <summary>
        /// Gets the RDF 'nil' URI node (end of list).
        /// </summary>
        protected IUriNode RdfNilNode => rdfNilNode.Value;

        /// <summary>
        /// Gets the RDF 'first' URI node (first element in a list).
        /// </summary>
        protected IUriNode RdfFirstNode => rdfFirstNode.Value;

        /// <summary>
        /// Gets the RDF 'rest' URI node (rest of the list).
        /// </summary>
        protected IUriNode RdfRestNode => rdfRestNode.Value;

        /// <summary>
        /// Initializes a new instance of the <see cref="BaseCollectionOperationStrategy"/> class.
        /// Sets up the graph update context and initializes RDF URI nodes.
        /// </summary>
        /// <param name="guc">The graph update context for the operation.</param>
        public BaseCollectionOperationStrategy(GraphUpdateContext guc)
        {
            GraphUpdateContext = guc;
            GraphManager = guc.GraphManager;

            // Lazy initialization of RDF URI nodes using the ontology graph.
            rdfAUriNode = new Lazy<IUriNode>(() => GraphManager.OntologyGraph.CreateUriNode("rdf:a"));
            rdfNilNode = new Lazy<IUriNode>(() => GraphManager.OntologyGraph.CreateUriNode("rdf:nil"));
            rdfFirstNode = new Lazy<IUriNode>(() => GraphManager.OntologyGraph.CreateUriNode("rdf:first"));
            rdfRestNode = new Lazy<IUriNode>(() => GraphManager.OntologyGraph.CreateUriNode("rdf:rest"));
        }
    }
}
