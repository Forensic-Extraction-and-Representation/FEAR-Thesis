using FEAR.Domain.KnowledgeGraph.GraphDB;
using Microsoft.Extensions.Configuration;

namespace FEAR.Domain.KnowledgeGraph.GraphDB.Cypher
{
    public class CypherDBConfiguration : GraphDBConfiguration
    {
        public CypherDBConfiguration(IConfiguration configuration) : base(configuration)
        {
        }
    }
}
