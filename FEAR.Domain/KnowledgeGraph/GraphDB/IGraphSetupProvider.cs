using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FEAR.Domain.KnowledgeGraph.GraphDB
{
    public interface IGraphSetupProvider
    {
        void ConfigureRemoteGraph(IServiceCollection cfg, IConfiguration configuration);
    }
}
