using System.Text;

namespace FEAR.Domain.KnowledgeGraph.GraphUpdate
{
    /// <summary>
    /// Represents a literal value node in the graph update structure.
    /// This node holds a property value and is associated with a parent entity and property.
    /// Computes a unique hash based on its value for identification and change tracking.
    /// </summary>
    public class GraphUpdateLiteral : GraphUpdateIdentifiableNode
    {
        /// <summary>
        /// Gets or sets the property value represented by this literal node.
        /// </summary>
        public PropertyValue NodeValue { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="GraphUpdateLiteral"/> class.
        /// </summary>
        /// <param name="value">The property value for this literal node.</param>
        /// <param name="parentReference">Reference to the parent entity and property.</param>
        /// <param name="identifiesParentByCondition">The identification condition for the parent.</param>
        public GraphUpdateLiteral(PropertyValue value, GraphUpdateEntityParentReference parentReference, IdentifiedByCondition identifiesParentByCondition)
        {
            NodeValue = value;
            ParentReference = parentReference;
            IdentifiesParentEntityByCondition = identifiesParentByCondition;

            Hash = ComputeHash();
        }

        /// <summary>
        /// Computes a unique hash for this literal node based on its value.
        /// Used for identification and change tracking in the graph update process.
        /// </summary>
        /// <returns>A base64-encoded string representing the hash of the literal value.</returns>
        private string ComputeHash()
        {
            List<string> components = new List<string>();
            components.Add(NodeValue.Value.ToString());

            return Convert.ToBase64String(sha256.ComputeHash(Encoding.UTF8.GetBytes(string.Join("", components))));
        }
    }
}