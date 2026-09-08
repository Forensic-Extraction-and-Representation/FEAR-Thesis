using VDS.RDF;
using VDS.RDF.Query;

namespace FEAR.Domain.KnowledgeGraph.Collections
{
    /// <summary>
    /// Represents the result of a collection operation, such as adding or removing triples
    /// or preparing a SPARQL query for execution.
    /// </summary>
    public class CollectionOperationResult
    {
        /// <summary>
        /// Gets or sets the set of triples to assert (add) or retract (remove) as part of the operation.
        /// </summary>
        public TriplesSet TripleSet { get; set; } = new TriplesSet();

        /// <summary>
        /// Gets or sets the SPARQL query to be executed as part of the operation, if applicable.
        /// </summary>
        public string Query { get; set; }

        public Dictionary<string, INode> QueryParameters { get; set; } = new Dictionary<string, INode>();

        /// <summary>
        /// Gets or sets a value indicating whether the operation requires execution of the SPARQL query.
        /// </summary>
        public bool RequiresExecute { get; set; } = false;
    }
}
