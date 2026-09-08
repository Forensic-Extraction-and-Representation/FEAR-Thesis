using FEAR.Domain.KnowledgeGraph.Graphs;
using FEAR.Runtime.DataFormats;

namespace FEAR.Runtime.Execution
{
    public class ReturningArtifactPipelineExecutionContext : IArtifactPipelineExecution
    {
        public Guid ExecutionInstanceId { get; } = Guid.NewGuid();

        public IDataFormatContextState State { get; set; }
        public Action<IEphemeralGraph, IArtifactPipelineExecution> OnCompleted { get; set; }
        public ReturningArtifactPipelineExecutionContext(IDataFormatContextState state, Action<IEphemeralGraph, IArtifactPipelineExecution> onCompleted)
        {
            State = state;
            OnCompleted = onCompleted;
        }
    }
}
