using FEAR.Domain.KnowledgeGraph.GraphUpdate;

namespace FEAR.Domain.KnowledgeGraph.EntitySearch
{
    /// <summary>
    /// Represents an entity that is to identified byother entities within a graph update operation.
    /// Stores a reference to the <see cref="GraphUpdateEntity"/> and its depth in the entity tree.
    /// </summary>
    public class IdentifiedEntity
    {
        /// <summary>
        /// The entity node in the graph update structure that is identified by another.
        /// </summary>
        public GraphUpdateEntity Entity { get; set; }

        /// <summary>
        /// The depth of the entity in the entity tree, where 0 is the root.
        /// </summary>
        public int Depth { get; set; }
    }
}