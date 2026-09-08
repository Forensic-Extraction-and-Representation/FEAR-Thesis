using FEAR.Domain.KnowledgeGraph.EntitySearch;
using FEAR.Domain.KnowledgeGraph.GraphDB;
using FEAR.Domain.KnowledgeGraph.GraphDB.Cypher;
using FEAR.Domain.KnowledgeGraph.Graphs;
using FEAR.Domain.KnowledgeGraph.TypeService;
using FEAR.Runtime.KnowledgeGraph.Cypher.EntitySearch;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace FEAR.Runtime.KnowledgeGraph.Cypher
{
    public class CypherSetupProvider : IGraphSetupProvider
    {
        public CypherSetupProvider() { }

        public void ConfigureRemoteGraph(IServiceCollection cfg, IConfiguration configuration)
        {
            cfg.AddTransient<IRemoteGraph<CypherParameterizedString>, CypherRemoteGraph>();

            cfg.AddScoped<IMaterializedGraph, CypherRemoteGraph>();

            cfg.AddSingleton<CypherDBConfiguration>();
            cfg.AddSingleton<CypherDBConnectionFactory>();
            cfg.AddTransient<IFindEntityStrategyFactory, PathFindEntityStrategyFactory>();

            cfg.RemoveAll<ITypeConversionService>();
            cfg.AddSingleton<ITypeConversionService, CypherTypeConversionService>();
        }
    }
}
