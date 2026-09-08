using FEAR.Domain.KnowledgeGraph.GraphCodify;

namespace FEAR.Domain.Collector
{
    /// <summary>
    /// Represents a result that contains a list of entities.
    /// </summary>
    public class EntityListResult : IEntityResult
    {
        public string InternalType => "List";
        public string EntityTypeName => "List";
        public List<CodifierExpandoObject> Entity { get; set; } = new List<CodifierExpandoObject>();
        public List<CodifierExpandoObject> Entities => Entity;

        /// <summary>
        /// Adds a new entry to the list of entities.
        /// </summary>
        /// <param name="entry">The new entity to add to the list</param>
        public void Add(CodifierExpandoObject entry)
        {
            Entity.Add(entry);
        }
    }
}
