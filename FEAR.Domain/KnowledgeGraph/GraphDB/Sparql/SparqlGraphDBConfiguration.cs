using Microsoft.Extensions.Configuration;

namespace FEAR.Domain.KnowledgeGraph.GraphDB.Sparql
{
    public class SparqlGraphDBConfiguration : GraphDBConfiguration
    {
        public SparqlGraphDBConfiguration(IConfiguration configuration) : base(configuration)
        {
        }
    }
}
