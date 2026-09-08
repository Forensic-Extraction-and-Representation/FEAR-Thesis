using VDS.RDF.Query;
using VDS.RDF;

namespace FEAR.Domain.KnowledgeGraph.GraphDB
{
    /// <summary>
    /// Defines a contract for connecting to and manipulating an RDF graph database.
    /// Provides methods for updating, querying, listing, and deleting graphs.
    /// </summary>
    public interface IGraphDBConnection
    {
        string GraphUri { get; }

        /// <summary>
        /// Gets a value indicating whether the connection supports update operations (adding/removing triples).
        /// </summary>
        bool UpdateSupported { get; }

        /// <summary>
        /// Gets a value indicating whether the connection supports deleting entire graphs.
        /// </summary>
        bool DeleteSupported { get; }

        /// <summary>
        /// Updates the specified graph by adding and/or removing triples, using a string graph URI.
        /// </summary>
        /// <param name="graphUri">The URI of the graph to update (as a string).</param>
        /// <param name="additions">Triples to add to the graph.</param>
        /// <param name="removals">Triples to remove from the graph.</param>
        void UpdateGraph(string graphUri, IEnumerable<Triple> additions, IEnumerable<Triple> removals);

        /// <summary>
        /// Updates the specified graph by adding and/or removing triples, using a <see cref="Uri"/> graph URI.
        /// </summary>
        /// <param name="graphUri">The URI of the graph to update.</param>
        /// <param name="additions">Triples to add to the graph.</param>
        /// <param name="removals">Triples to remove from the graph.</param>
        void UpdateGraph(Uri graphUri, IEnumerable<Triple> additions, IEnumerable<Triple> removals);

        /// <summary>
        /// Lists all graph URIs available in the database.
        /// </summary>
        /// <returns>An enumerable of <see cref="Uri"/> representing the available graphs.</returns>
        IEnumerable<Uri> ListGraphs();

        /// <summary>
        /// Deletes the specified graph from the database.
        /// </summary>
        /// <param name="graphUri">The URI of the graph to delete.</param>
        void DeleteGraph(Uri graphUri);

        void Update(string queryString);
    }
}
