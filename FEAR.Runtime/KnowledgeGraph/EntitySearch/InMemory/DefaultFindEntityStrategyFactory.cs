using FEAR.Domain.KnowledgeGraph.EntitySearch;
using FEAR.Domain.KnowledgeGraph.Graphs;
using FEAR.Domain.KnowledgeGraph.TypeService;

namespace FEAR.Runtime.KnowledgeGraph.EntitySearch.InMemory
{
    public class DefaultFindEntityStrategyFactory : ILocalGraphFindEntityStrategyFactory
    {
        private readonly ITypeConversionService TypeConversionService;
        public DefaultFindEntityStrategyFactory(ITypeConversionService typeConversionService)
        {
            TypeConversionService = typeConversionService;
        }

        public IFindEntityStrategy CreateStrategy(IRealGraph graph)
        {
            return new DefaultFindEntityStrategy(graph, TypeConversionService);
        }
    }
}
