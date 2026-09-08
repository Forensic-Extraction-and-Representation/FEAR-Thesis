using System.Security.Cryptography;

namespace FEAR.Domain.KnowledgeGraph.GraphUpdate
{
    /// <summary>
    /// Abstract base class for all nodes in the graph update structure.
    /// Provides a common interface for hash computation and parent relationship tracking.
    /// </summary>
    public abstract class GraphUpdateNode
    {
        /// <summary>
        /// Shared SHA256 instance for computing hashes of node contents.
        /// </summary>
        protected static SHA256 sha256 = SHA256.Create();

        /// <summary>
        /// Gets the hash value representing the contents or identity of this node.
        /// Used for change tracking and identification within the graph update process.
        /// </summary>
        public string Hash { get; protected set; }

        /// <summary>
        /// Gets or sets the reference to the parent entity and property that this node is related to.
        /// </summary>
        public GraphUpdateEntityParentReference ParentReference { get; set; }
    }
}