using FEAR.Domain.KnowledgeGraph.GraphDB;
using Microsoft.Extensions.Configuration;

namespace FEAR.Domain.KnowledgeGraph.GraphDB.GraphQL
{
    public class GraphQLDBConfiguration : GraphDBConfiguration
    {
        public GraphQLDBConfiguration(IConfiguration configuration) : base(configuration)
        {
        }
    }
}
