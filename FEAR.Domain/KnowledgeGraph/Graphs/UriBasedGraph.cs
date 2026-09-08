using FEAR.Domain.KnowledgeGraph.EntitySearch;
using FEAR.Domain.KnowledgeGraph.GraphManager;
using FEAR.Domain.KnowledgeGraph.GraphUpdate;
using System.Collections.Concurrent;
using VDS.RDF;

namespace FEAR.Domain.KnowledgeGraph.Graphs
{
    /// <summary>
    /// Abstract base class for URI-based RDF graphs.
    /// Implements caching and node lookup logic, and provides a foundation for materialized or remote graph types.
    /// </summary>
    public abstract class UriBasedGraph : IUriBasedGraph
    {
        /// <summary>
        /// Gets or sets a value indicating whether triple assertions and retractions should be tracked.
        /// </summary>
        public bool TrackTriples { get; set; } = true;

        /// <summary>
        /// Gets the strategy used to find entities in the graph.
        /// Must be implemented by derived classes.
        /// </summary>
        protected abstract IFindEntityStrategy FindEntityStrategy { get; }

        /// <summary>
        /// Gets the underlying ontology graph instance.
        /// Must be implemented by derived classes.
        /// </summary>
        public abstract VDS.RDF.Ontology.OntologyGraph Graph { get; }

        /// <summary>
        /// Gets or sets the graph manager responsible for this graph instance.
        /// </summary>
        public IGraphManager GraphManager { get; set; }

        // Thread-safe cache for URI nodes to avoid redundant node creation.
        private readonly ConcurrentDictionary<string, INode> nodeCache = new ConcurrentDictionary<string, INode>();

        /// <summary>
        /// Initializes a new instance of the <see cref="UriBasedGraph"/> class.
        /// </summary>
        public UriBasedGraph()
        {
        }

        /// <summary>
        /// Creates or retrieves a URI node for the specified <see cref="Uri"/>.
        /// </summary>
        /// <param name="uri">The URI to create a node for.</param>
        /// <returns>The <see cref="INode"/> representing the URI, or null if creation fails.</returns>
        public INode? CreateUriNode(Uri uri)
        {
            return Graph.CreateUriNode(uri);
        }

        /// <summary>
        /// Finds an entity in the graph using the provided update context.
        /// Must be implemented by derived classes.
        /// </summary>
        /// <param name="entityContext">The update context containing search parameters.</param>
        /// <returns>A <see cref="KGResponse{Entity}"/> containing the found entity or error information.</returns>
        public abstract KGResponse<Entity> FindEntity(GraphUpdateContext entityContext);

        /// <summary>
        /// Retrieves the RDF node corresponding to the specified URI string.
        /// Resolves prefix notation using the graph manager, then delegates to <see cref="GetNodeForUri(Uri)"/>.
        /// </summary>
        /// <param name="uri">The URI string or prefix notation.</param>
        /// <returns>The <see cref="INode"/> associated with the given URI string.</returns>
        public INode GetNodeForUri(string uri)
        {
            var fullUri = GraphManager.GetUriFromPrefixNotation(uri).Value;
            return GetNodeForUri(fullUri);
        }

        /// <summary>
        /// Retrieves the RDF node corresponding to the specified <see cref="Uri"/>.
        /// Uses a thread-safe cache to avoid redundant node creation.
        /// </summary>
        /// <param name="uri">The URI of the node to retrieve.</param>
        /// <returns>The <see cref="INode"/> associated with the given URI.</returns>
        public INode GetNodeForUri(Uri uri)
        {
            if (nodeCache.TryGetValue(uri.AbsoluteUri, out INode node))
                return node;
            else
            {
                var newNode = Graph.CreateUriNode(uri);
                nodeCache.TryAdd(uri.AbsoluteUri, newNode);
                return newNode;
            }
        }
    }
}
