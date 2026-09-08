namespace FEAR.Domain.KnowledgeGraph.GraphUpdate
{
    /// <summary>
    /// Represents a reference within a specific knowledge graph via a URI.
    /// </summary>
    public class GraphReference
    {
        /// <summary>
        /// Gets or sets the URI that uniquely identifies the target entity graph.
        /// </summary>
        public Uri EntityGraphUri { get; set; }
    }
}