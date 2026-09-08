namespace FEAR.Domain.KnowledgeGraph.GraphUpdate
{
    /// <summary>
    /// Represents a node in the graph update structure that 
    /// can be uniquely identified by entities or properties.
    /// </summary>
    public class GraphUpdateIdentifiableNode : GraphUpdateNode
    {
        /// <summary>
        /// Gets or sets the reference to the entity in the graph
        /// that is associated with this node.
        /// </summary>
        public GraphReference? EntityGraphReference { get; set; }

        /// <summary>
        /// Gets or sets the condition describing how this node identifies its parent entity.
        /// For example, if this node is a required property of the parent entity, 
        /// it would be set to <see cref="IdentifiedByCondition.Required"/>.
        /// If there is no direct identification condition, it would be set to <see cref="IdentifiedByCondition.None"/>.
        /// </summary>
        public IdentifiedByCondition IdentifiesParentEntityByCondition { get; set; } = IdentifiedByCondition.None;

        /// <summary>
        /// Returns a string representation of the node, showing its relationship to the parent
        /// and identification condition. If the node is a <see cref="GraphUpdateEntity"/> or
        /// <see cref="GraphUpdateLiteral"/>, returns a more specific identifier.
        /// </summary>
        /// <returns>A string describing the node.</returns>
        public override string ToString()
        {
            var s = $"{ParentReference.PropertyName} - {IdentifiesParentEntityByCondition} - ";
            if (this is GraphUpdateEntity)
            {
                s = $"{(this as GraphUpdateEntity).EntityQueryIdentifier}";
            }
            else if (this is GraphUpdateLiteral)
            {
                s = $"{(this as GraphUpdateLiteral).NodeValue.PropertyName}";
            }

            return s;
        }
    }
}