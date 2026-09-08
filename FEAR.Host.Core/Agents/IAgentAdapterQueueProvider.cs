using FEAR.Domain.Agents;
using FEAR.Host.Core.Services;

namespace FEAR.Host.Core.Agents
{
    public class QueuedAgentRequest
    {
        public QueuedAgentRequest(AgentExecutionContext ctxFromProvider, AgentInteraction ctxEntry)
        {
            Context = ctxFromProvider;
            PromptContext = ctxEntry;
        }

        public AgentExecutionContext Context { get; set; }
        public AgentInteraction PromptContext { get; set; }
    }

    public interface IAgentAdapterQueueProvider
    {
        void Enqueue(QueuedAgentRequest request);
        bool TryDequeue(out QueuedAgentRequest request);
    }
}
