using Microsoft.Extensions.Configuration;

namespace FEAR.Domain.KnowledgeGraph.GraphDB.Fuseki
{
    public class FusekiGraphDBConfiguration : GraphDBConfiguration
    {
        public FusekiGraphDBConfiguration(IConfiguration configuration) : base(configuration)
        {
        }
    }
}
