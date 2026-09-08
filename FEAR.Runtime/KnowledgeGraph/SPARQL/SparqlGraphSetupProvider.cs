using FEAR.Domain.KnowledgeGraph.EntitySearch;
using FEAR.Domain.KnowledgeGraph.GraphDB;
using FEAR.Domain.KnowledgeGraph.GraphDB.Sparql;
using FEAR.Domain.KnowledgeGraph.Graphs;
using FEAR.Runtime.KnowledgeGraph.Sparql.EntitySearch;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using VDS.RDF.Query;

namespace FEAR.Runtime.KnowledgeGraph.Sparql
{
    public class SparqlGraphSetupProvider : IGraphSetupProvider
    {
        public SparqlGraphSetupProvider() { }

        public void ConfigureRemoteGraph(IServiceCollection cfg, IConfiguration configuration)
        {
            cfg.AddTransient<IRemoteGraph<SparqlParameterizedString>, SparqlRemoteGraph>();

            cfg.AddScoped<IMaterializedGraph, SparqlRemoteGraph>();

            cfg.AddSingleton<SparqlGraphDBConfiguration>();
            cfg.AddSingleton<SparqlGraphDBConnectionFactory>();
            cfg.AddTransient<IFindEntityStrategyFactory, PathFindEntityStrategyFactory>();
        }
    }
}
