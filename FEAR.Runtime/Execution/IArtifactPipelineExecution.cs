using FEAR.Domain.KnowledgeGraph.Graphs;
using FEAR.Runtime.DataFormats;

namespace FEAR.Runtime.Execution
{
    public interface IArtifactPipelineExecution
    {
        Guid ExecutionInstanceId { get; }
        IDataFormatContextState State { get; set; }
        Action<IEphemeralGraph, IArtifactPipelineExecution> OnCompleted { get; set; }
    }
}
