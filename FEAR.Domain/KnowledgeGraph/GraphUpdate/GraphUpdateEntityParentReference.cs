namespace FEAR.Domain.KnowledgeGraph.GraphUpdate
{
    /// <summary>
    /// Represents a reference to a parent entity and the property through which a relationship is established.
    /// Used in the graph update structure to track the parent-child relationship between entities or collections.
    /// </summary>
    public class GraphUpdateEntityParentReference
    {
        /// <summary>
        /// Gets or sets the parent entity in the relationship.
        /// This is the entity that owns the property referencing the current node.
        /// </summary>
        public GraphUpdateEntity? ParentEntity { get; set; }

        /// <summary>
        /// Gets or sets the name of the property on the parent entity that establishes the relationship.
        /// </summary>
        public string PropertyName { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="GraphUpdateEntityParentReference"/> class.
        /// </summary>
        public GraphUpdateEntityParentReference()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GraphUpdateEntityParentReference"/> class
        /// with the specified parent entity and property name.
        /// </summary>
        /// <param name="parentEntity">The parent entity in the relationship.</param>
        /// <param name="propertyName">The property name on the parent entity.</param>
        public GraphUpdateEntityParentReference(GraphUpdateEntity parentEntity, string propertyName)
        {
            ParentEntity = parentEntity;
            PropertyName = propertyName;
        }
    }
}