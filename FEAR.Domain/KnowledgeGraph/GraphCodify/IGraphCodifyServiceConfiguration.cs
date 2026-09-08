namespace FEAR.Domain.KnowledgeGraph.GraphCodify
{
    /// <summary>
    /// Configuration options for the Graph Codify Service.
    /// Controls how data conversion is handled during codification.
    /// </summary>
    public interface IGraphCodifyServiceConfiguration
    {
        /// <summary>
        /// Gets or sets a value indicating whether strict type conversion should be enforced
        /// during codification. If true, only the Type field is used to match an artifact
        /// to scripts for codification. If false, more permissive match on field names is allowed.
        /// </summary>
        bool StrictConversion { get; }
    }
}
