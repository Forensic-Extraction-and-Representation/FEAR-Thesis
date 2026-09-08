using FEAR.Domain.KnowledgeGraph.GraphCodify;
using FEAR.Domain.KnowledgeGraph.Graphs;
using FEAR.Domain.KnowledgeGraph.IRI;
using VDS.RDF.Ontology;

namespace FEAR.Domain.KnowledgeGraph.GraphManager
{
    /// <summary>
    /// Defines a contract for managing ontology and materialized graphs, IRI generation, and namespace handling.
    /// Provides methods for creating codification services, IRIs, and temporary graphs.
    /// </summary>
    public interface IGraphManager : IUriBasedGraph
    {
        /// <summary>
        /// Gets the ontology graph, which contains schema and vocabulary definitions.
        /// </summary>
        OntologyGraph OntologyGraph { get; }

        /// <summary>
        /// Gets the materialized graph, which contains the final graph of all data and relationships.
        /// This could be a remote graph or a local graph depending on the implementation.
        /// </summary>
        IMaterializedGraph MaterializedGraph { get; }

        /// <summary>
        /// Gets the IRI generator used for creating unique IRIs for entities.
        /// </summary>
        IIRIGenerator IRIGenerator { get; }

        /// <summary>
        /// Resolves a URI from a prefix notation string (e.g., "rdf:type").
        /// </summary>
        /// <param name="prefix">The prefixed string to resolve.</param>
        /// <returns>A <see cref="KGResponse{Uri}"/> containing the resolved URI or error information.</returns>
        KGResponse<Uri> GetUriFromPrefixNotation(string prefix);

        /// <summary>
        /// Adds a namespace mapping to the ontology graph.
        /// </summary>
        /// <param name="prefix">The namespace prefix.</param>
        /// <param name="uri">The namespace URI.</param>
        void AddNamespace(string prefix, Uri uri);

        /// <summary>
        /// Creates a new graph codification service for entity and relationship management.
        /// </summary>
        /// <returns>An <see cref="IGraphCodifyService"/> instance.</returns>
        IGraphCodifyService CreateGraphCodifyService();

        /// <summary>
        /// Creates a new IRI for an entity, using the specified type and URI segment.
        /// </summary>
        /// <param name="type">The type of entity (used in the IRI if no segment is provided).</param>
        /// <param name="uriSegment">A custom segment for the IRI. If null or empty, a unique segment should be generated.</param>
        /// <returns>A new <see cref="Uri"/> representing the entity's IRI.</returns>
        Uri CreateIRI(string type, string uriSegment);

        /// <summary>
        /// Creates a temporary local graph for a specific purpose and description.
        /// </summary>
        /// <param name="purpose">The purpose of the temporary graph.</param>
        /// <param name="description">A description of the temporary graph.</param>
        /// <returns>An <see cref="ILocalGraph"/> instance.</returns>
        ILocalGraph CreateTemporaryGraph(string purpose, string description);
    }
}
