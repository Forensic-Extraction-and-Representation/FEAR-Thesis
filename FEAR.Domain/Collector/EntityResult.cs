using FEAR.Domain.KnowledgeGraph.GraphCodify;

namespace FEAR.Domain.Collector
{
    /// <summary>
    /// Represents a result that contains a single entity.
    /// </summary>
    public class EntityResult : IEntityResult
    {
        public string InternalType => EntityTypeName;
        public string EntityTypeName { get; set; }
        public dynamic Entity { get; set; } = new CodifierExpandoObject();
    }
}
