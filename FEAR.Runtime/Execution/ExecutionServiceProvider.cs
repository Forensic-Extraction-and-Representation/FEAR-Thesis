using FEAR.Domain.Agents;
using FEAR.Domain.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace FEAR.Runtime.Execution
{
    public class ExecutionServiceProvider : IExecutionServiceProvider
    {
        private IServiceProvider _serviceProvider;
        private Lazy<AgentAdapterRegistry> _agentAdapterProvider;
        public IAgentAdapterRegistry AgentProvider => _agentAdapterProvider.Value;

        public ExecutionServiceProvider(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            _agentAdapterProvider = new Lazy<AgentAdapterRegistry>(() => _serviceProvider.GetService<AgentAdapterRegistry>());
        }
    }
}
