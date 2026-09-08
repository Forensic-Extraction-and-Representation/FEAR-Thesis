using FEAR.Domain.KnowledgeGraph.GraphDB.Cypher;
using FEAR.Domain.KnowledgeGraph.Graphs;
using FEAR.Domain.KnowledgeGraph.TypeService;
using FEAR.Runtime.KnowledgeGraph.EntitySearch;

namespace FEAR.Runtime.KnowledgeGraph.Cypher.EntitySearch
{
    public class PathFindEntityStrategy : AbstractPathFindEntityStrategy<CypherParameterizedString>
    {
        public PathFindEntityStrategy(IRealGraph completeGraph, ITypeConversionService typeConversionService) 
            : base(completeGraph, typeConversionService) 
        {
        }

        public override BaseConstructQueryGenerator<CypherParameterizedString> CreateQueryGenerator(IRealGraph completeGraph, ITypeConversionService typeConversionService)
        {
            return new ConstructQueryGenerator(CompleteGraph, TypeConversionService);
        }
    }
}