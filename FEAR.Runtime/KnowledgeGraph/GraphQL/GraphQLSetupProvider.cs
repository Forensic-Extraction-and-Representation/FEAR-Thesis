using FEAR.Domain.KnowledgeGraph.EntitySearch;
using FEAR.Domain.KnowledgeGraph.GraphDB;
using FEAR.Domain.KnowledgeGraph.GraphDB.GraphQL;
using FEAR.Domain.KnowledgeGraph.Graphs;
using FEAR.Runtime.KnowledgeGraph.GraphQL.EntitySearch;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using VDS.RDF.Query;

namespace FEAR.Runtime.KnowledgeGraph.GraphQL
{
    public class GraphQLSetupProvider : IGraphSetupProvider
    {
        public GraphQLSetupProvider() { }

        public void ConfigureRemoteGraph(IServiceCollection cfg, IConfiguration configuration)
        {
            cfg.AddTransient<IRemoteGraph<string>, GraphQLRemoteGraph>();

            cfg.AddScoped<IMaterializedGraph, GraphQLRemoteGraph>();

            cfg.AddSingleton<GraphQLDBConfiguration>();
            cfg.AddSingleton<GraphQLDBConnectionFactory>();
            cfg.AddTransient<IFindEntityStrategyFactory, PathFindEntityStrategyFactory>();
        }
    }
}
