using FEAR.Domain.KnowledgeGraph;
using FEAR.Domain.KnowledgeGraph.Graphs;
using FEAR.Domain.KnowledgeGraph.GraphUpdate;
using FEAR.Domain.KnowledgeGraph.TypeService;
using FEAR.Runtime.KnowledgeGraph.EntitySearch;
using FEAR.Runtime.KnowledgeGraph.EntitySearch.InMemory;
using VDS.RDF;
using VDS.RDF.Query;

namespace FEAR.Runtime.KnowledgeGraph.Sparql.EntitySearch
{
    public partial class PathFindEntityStrategy : AbstractPathFindEntityStrategy<SparqlParameterizedString>
    {
        public PathFindEntityStrategy(IRealGraph completeGraph, ITypeConversionService typeConversionService)
        : base(completeGraph, typeConversionService)
        {
        }

        public override BaseConstructQueryGenerator<SparqlParameterizedString> CreateQueryGenerator(IRealGraph completeGraph, ITypeConversionService typeConversionService)
        {
            return new ConstructQueryGenerator(CompleteGraph, TypeConversionService);
        }
    }
}