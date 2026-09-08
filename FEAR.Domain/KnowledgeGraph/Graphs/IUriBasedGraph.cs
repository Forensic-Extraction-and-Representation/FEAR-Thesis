using VDS.RDF;
using VDS.RDF.Ontology;

namespace FEAR.Domain.KnowledgeGraph.Graphs
{
    /// <summary>
    /// Defines a contract for graph implementations that are based on URIs (such as RDF graphs).
    /// Extends <see cref="INodeBasedGraph"/> to provide access to the underlying ontology graph
    /// and to support creation of URI nodes.
    /// </summary>
    public interface IUriBasedGraph : INodeBasedGraph
    {
        /// <summary>
        /// Gets the underlying graph instance.
        /// </summary>
        OntologyGraph Graph { get; }

        /// <summary>
        /// Creates or retrieves a URI node for the specified <see cref="Uri"/>.
        /// </summary>
        /// <param name="uri">The URI to create a node for.</param>
        /// <returns>The <see cref="INode"/> representing the URI, or null if creation fails.</returns>
        INode? CreateUriNode(Uri uri);
    }
}
