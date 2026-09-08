using VDS.RDF;

namespace FEAR.Domain.KnowledgeGraph
{
    /// <summary>
    /// Represents a set of RDF triples to be asserted (added) or retracted (removed) in a knowledge graph operation.
    /// Used to batch changes for graph updates, synchronization, or transaction management.
    /// </summary>
    public class TriplesSet
    {
        /// <summary>
        /// Gets or sets the list of triples to assert (add) to the graph.
        /// </summary>
        public IList<Triple> Assert { get; set; } = new List<Triple>();

        /// <summary>
        /// Gets or sets the list of triples to retract (remove) from the graph.
        /// </summary>
        public IList<Triple> Retract { get; set; } = new List<Triple>();
    }
}
