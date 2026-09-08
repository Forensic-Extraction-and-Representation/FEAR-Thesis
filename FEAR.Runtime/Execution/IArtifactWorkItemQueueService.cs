using FEAR.Domain.KnowledgeGraph.GraphManager;
using FEAR.Domain.Provenance;

namespace FEAR.Runtime.Execution
{
    /// <summary>
    /// Defines a contract for a queue service that manages data to be processed by graph codify scripts.
    /// 
    /// The queue can contain:
    ///   - Artifacts directly (raw evidence or input data)
    ///   - Results produced by a collector (CFEAR) script
    /// 
    /// This interface ensures that all relevant data, regardless of whether it is a direct artifact or a collector result,
    /// is queued for processing by the appropriate graph codifiers, enabling knowledge extraction and transformation workflows.
    /// </summary>
    public interface IArtifactWorkItemQueueService : IQueueService
    {
        /// <summary>
        /// Gets the graph manager responsible for managing ontology and materialized graphs.
        /// </summary>
        IGraphManager GraphManager { get; }

        /// <summary>
        /// Queues provenance information for processing. This can be used to track the origin and history of artifacts or results.
        /// </summary>
        /// <param name="provenance">The provenance object to queue.</param>
        void QueueProvenance(IProvenance provenance);

        /// <summary>
        /// Queues a collector result (either a direct artifact or the output of a collector/CFEAR script)
        /// to be processed by graph codify scripts.
        /// </summary>
        /// <param name="result">The collector result to queue for processing.</param>
        void QueueArtifactWorkItem(ArtifactWorkItem result);
    }
}
