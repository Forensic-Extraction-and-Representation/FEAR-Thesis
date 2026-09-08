using VDS.RDF;

namespace FEAR.Domain.KnowledgeGraph.Graphs
{
    /// <summary>
    /// Defines a contract for graph implementations are based on Nodes from dotNetRdf.
    /// Allows retrieval of RDF nodes (<see cref="INode"/>) using either a <see cref="Uri"/> or string representation.
    /// Useful for mapping URIs to nodes in RDF graphs, supporting both strongly-typed and string-based access.
    /// </summary>
    public interface INodeBasedGraph
    {
        /// <summary>
        /// Retrieves the RDF node corresponding to the specified <see cref="Uri"/>.
        /// </summary>
        /// <param name="uri">The URI of the node to retrieve.</param>
        /// <returns>The <see cref="INode"/> associated with the given URI, or null if not found.</returns>
        INode GetNodeForUri(Uri uri);

        /// <summary>
        /// Retrieves the RDF node corresponding to the specified URI string.
        /// </summary>
        /// <param name="uri">The URI string of the node to retrieve.</param>
        /// <returns>The <see cref="INode"/> associated with the given URI string, or null if not found.</returns>
        INode GetNodeForUri(string uri);
    }
}
