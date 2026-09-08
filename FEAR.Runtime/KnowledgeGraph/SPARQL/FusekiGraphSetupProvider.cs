using FEAR.Domain.KnowledgeGraph.EntitySearch;
using FEAR.Domain.KnowledgeGraph.GraphDB;
using FEAR.Domain.KnowledgeGraph.GraphDB.Fuseki;
using FEAR.Domain.KnowledgeGraph.Graphs;
using FEAR.Runtime.KnowledgeGraph.Sparql;
using FEAR.Runtime.KnowledgeGraph.Sparql.EntitySearch;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using VDS.RDF.Query;

namespace FEAR.Runtime.KnowledgeGraph.Sparql
{
    public class FusekiGraphSetupProvider : IGraphSetupProvider
    {
        public FusekiGraphSetupProvider() { }

        public void ConfigureRemoteGraph(IServiceCollection cfg, IConfiguration configuration)
        {
            cfg.AddTransient<IRemoteGraph<SparqlParameterizedString>, FusekiRemoteGraph>();

            cfg.AddScoped<IMaterializedGraph, FusekiRemoteGraph>();

            cfg.AddSingleton<FusekiGraphDBConfiguration>();
            cfg.AddSingleton<FusekiGraphDBConnectionFactory>();
            cfg.AddTransient<IFindEntityStrategyFactory, PathFindEntityStrategyFactory>();
        }
    }
}
