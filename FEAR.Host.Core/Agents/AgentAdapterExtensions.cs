using FEAR.Domain.Agents;
using FEAR.Host.Core.Services;
using FEAR.Runtime.Agents;
using FEAR.Runtime.Execution;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Security.Claims;

namespace FEAR.Host.Core.Agents
{
    public static class AgentAdapterExtensions
    {
        public static HostApplicationBuilder AddAgentAdapters(this HostApplicationBuilder builder, DataSigningService dataSigningService)
        {
            builder.Services.AddSingleton(dataSigningService);
            builder.Services.AddSingleton<AgentAdapterContextProvider>();
            builder.Services.AddSingleton<IAgentAdapterQueueProvider, AgentAdapterQueueProvider>();
            builder.Services.AddSingleton<IAgentAdapterRegistry, AgentAdapterRegistry>();
            builder.Services.AddHostedService<AgentExecutionService<ClaimsPrincipal>>();
            
            return builder;
        }

    }
}
