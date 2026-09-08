using FEAR.Domain.KnowledgeGraph.EntitySearch;
using FEAR.Domain.KnowledgeGraph.Graphs;
using FEAR.Domain.KnowledgeGraph.GraphUpdate;

namespace FEAR.Runtime.KnowledgeGraph.EntitySearch
{
    public interface IConstructQueryGenerator<T>
    {
        T ConstructLiteralEntityLoadQuery(ICollection<GraphUpdateEntity> leafs);
        T ConstructNodeEntityLoadQuery(Dictionary<IdentifiedEntity, int> flattenedTree, IList<GraphUpdateCollection> collections, int maxDepth);
        T LoadListCollections(GraphUpdateCollection collection);
        void LoadLeafUriFromGraph(ICollection<GraphUpdateEntity> leafs, IMaterializedGraph tempGraph);
    }
}