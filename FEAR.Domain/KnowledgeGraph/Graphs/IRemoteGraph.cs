using VDS.RDF.Query;

namespace FEAR.Domain.KnowledgeGraph.Graphs
{
    public interface IRemoteGraph
    {

        /// <summary>
        /// Asserts and/or retracts triples in the remote graph as specified by the given set.
        /// This operation is performed directly against the remote store.
        /// </summary>
        /// <param name="triplesSet">The set of triples to assert or retract.</param>
        void AssertAndRetractRemote(TriplesSet triplesSet);
    }

    /// <summary>
    /// Represents a remote, materialized graph that supports advanced operations such as remote assertion/retraction
    /// and SPARQL CONSTRUCT queries. Inherits from <see cref="IMaterializedGraph"/> and is intended for use with
    /// graphs that are persisted or managed remotely (e.g., in a triple store or SPARQL endpoint).
    /// </summary>
    public interface IRemoteGraph<T> : IRemoteGraph, IMaterializedGraph
    {
        /// <summary>
        /// Executes a SPARQL CONSTRUCT query against the remote graph.
        /// The results are typically used to build a new graph or extract subgraphs.
        /// </summary>
        /// <param name="query">The SPARQL CONSTRUCT query to execute.</param>
        void ExecuteConstructQuery(T query);

        /// <summary>
        /// Executes a SPARQL CONSTRUCT query against the remote graph and populates the specified result graph.
        /// </summary>
        /// <param name="query">The SPARQL CONSTRUCT query to execute.</param>
        /// <param name="resultGraph">The graph to populate with the results of the CONSTRUCT query.</param>
        void ExecuteConstructQuery(T query, IRealGraph resultGraph);
    }
}
