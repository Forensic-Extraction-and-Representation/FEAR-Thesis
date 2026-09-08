using FEAR.Domain.Agents;

namespace FEAR.Domain.Infrastructure
{
    public interface IExecutionServiceProvider
    {
        IAgentAdapterRegistry AgentProvider { get; }
    }
}
