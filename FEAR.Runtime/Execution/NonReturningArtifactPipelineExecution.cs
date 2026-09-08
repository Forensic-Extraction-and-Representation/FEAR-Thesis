using FEAR.Domain.KnowledgeGraph.Graphs;
using FEAR.Runtime.DataFormats;

namespace FEAR.Runtime.Execution
{
    public class NonReturningArtifactPipelineExecution : IArtifactPipelineExecution
    {
        public Guid ExecutionInstanceId { get; } = Guid.NewGuid();
        public IDataFormatContextState State { get; set; }
        public Action<IEphemeralGraph, IArtifactPipelineExecution> OnCompleted { get; set; }
        public NonReturningArtifactPipelineExecution(IDataFormatContextState state)
        {
            State = state;
            OnCompleted = (obj, ctx) => { };
        }
    }
}
