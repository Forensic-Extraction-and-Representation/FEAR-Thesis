using VDS.RDF;

namespace FEAR.Domain.Agents.Attachments
{
    /// <summary>
    /// Represents a graph attachment with TTL format content.
    /// </summary>
    public class GraphAttachment : SignedAgentAttachment
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GraphAttachment"/> class.
        /// </summary>
        public GraphAttachment()
        {
            Format = "TTL";
            Name = "Graph";
        }
    }
}
