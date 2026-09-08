using FEAR.Domain.KnowledgeGraph;

namespace FEAR.Runtime.KnowledgeGraph.EntitySearch.InMemory
{
    public class EntitySearchResult
    {
        public Entity Entity { get; set; }
        public int OptionalCount { get; set; }
        public bool RequiredFound { get; set; }

        public EntitySearchResult() { }

        public EntitySearchResult(Entity entity)
        {
            Entity = entity;
        }
    }
}
