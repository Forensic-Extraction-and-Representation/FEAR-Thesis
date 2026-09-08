using FEAR.Domain.KnowledgeGraph.EntitySearch;
using FEAR.Domain.KnowledgeGraph.Graphs;
using FEAR.Domain.KnowledgeGraph.TypeService;

namespace FEAR.Runtime.KnowledgeGraph.Sparql.EntitySearch
{
    public class PathFindEntityStrategyFactory : IRemoteGraphFindEntityStrategyFactory
    {
        private readonly ITypeConversionService TypeConversionService;
        public PathFindEntityStrategyFactory(ITypeConversionService typeConversionService)
        {
           TypeConversionService = typeConversionService;
        }

        public IFindEntityStrategy CreateStrategy(IRealGraph graph)
        {
            return new PathFindEntityStrategy(graph, TypeConversionService);
        }
    }
}
