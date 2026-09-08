using VDS.RDF;
using VDS.RDF.Configuration;
using VDS.RDF.Query;
using VDS.RDF.Storage;
using VDS.RDF.Update;

namespace FEAR.Domain.KnowledgeGraph.GraphDB.Sparql
{
    /// <summary>
    /// Provides a connection to a Sparql-backed RDF graph database using dotNetRDF's SparqlConnector.
    /// Implements the <see cref="IGraphDBConnection"/> interface for graph operations such as update, delete, and query.
    /// </summary>
    public class SparqlGraphDBConnection : IGraphDBConnection
    {
        // Configuration settings for the Sparql connection.
        private GraphDBConfiguration configuration;

        // Lazily-initialized SparqlConnector instance for communicating with the Sparql server.
        private Lazy<OntotextGraphDBConnector> connection = null;

        /// <summary>
        /// Gets the underlying SparqlConnector instance.
        /// </summary>
        public OntotextGraphDBConnector Connection => connection.Value;

        /// <summary>
        /// Gets the configuration used for this connection.
        /// </summary>
        private GraphDBConfiguration Configuration { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="GraphQLGraphDBConnection"/> class.
        /// Sets up the connection and applies authentication if required.
        /// </summary>
        /// <param name="configuration">The configuration for the Sparql connection.</param>
        public SparqlGraphDBConnection(GraphDBConfiguration configuration)
        {
            Configuration = configuration;
            connection = new Lazy<OntotextGraphDBConnector>(() =>
            {
                var conn = new OntotextGraphDBConnector(Configuration.Endpoint);
                GraphUri = Configuration.GraphUri;

                if (Configuration.Authentication)
                {
                    conn.SetCredentials(Configuration.Username, Configuration.Password);
                }
                return conn;
            });
        }

        /// <summary>
        /// Gets a value indicating whether update operations are supported by the connection.
        /// </summary>
        public bool UpdateSupported => Connection.UpdateSupported;

        /// <summary>
        /// Gets a value indicating whether delete operations are supported by the connection.
        /// </summary>
        public bool DeleteSupported => Connection.DeleteSupported;

        /// <summary>
        /// Gets the URI of the graph this connection is associated with.
        /// </summary>
        public string GraphUri { get; private set; }

        /// <summary>
        /// Updates the specified graph by adding and/or removing triples.
        /// </summary>
        /// <param name="graphUri">The URI of the graph to update.</param>
        /// <param name="additions">Triples to add.</param>
        /// <param name="removals">Triples to remove.</param>
        public void UpdateGraph(string graphUri, IEnumerable<Triple> additions, IEnumerable<Triple> removals) => Connection.UpdateGraph(graphUri, additions, removals);

        /// <summary>
        /// Creates a SPARQL query processor for executing queries against the Sparql endpoint.
        /// </summary>
        /// <returns>An <see cref="ISparqlQueryProcessor"/> instance.</returns>
        public ISparqlQueryProcessor CreateSparqlQueryProcessor() => new GenericQueryProcessor(Connection);

        /// <summary>
        /// Updates the specified graph by adding and/or removing triples (using a Uri overload).
        /// </summary>
        /// <param name="graphUri">The URI of the graph to update.</param>
        /// <param name="additions">Triples to add.</param>
        /// <param name="removals">Triples to remove.</param>
        public void UpdateGraph(Uri graphUri, IEnumerable<Triple> additions, IEnumerable<Triple> removals) => Connection.UpdateGraph(graphUri, additions, removals);

        /// <summary>
        /// Lists all graph URIs available in the Sparql store.
        /// </summary>
        /// <returns>An enumerable of graph URIs.</returns>
        public IEnumerable<Uri> ListGraphs() => Connection.ListGraphs();

        /// <summary>
        /// Deletes the specified graph from the Sparql store.
        /// </summary>
        /// <param name="graphUri">The URI of the graph to delete.</param>
        public void DeleteGraph(Uri graphUri) => Connection.DeleteGraph(graphUri);

        public void Update(string queryString)
        {
            // This method is not implemented in the original code, but can be used to execute SPARQL update queries.
            // It could be implemented using Connection.Update(queryString) if needed.
            Connection.Update(queryString);
        }
    }
}
