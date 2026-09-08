namespace FEAR.Domain.KnowledgeGraph.Graphs
{
    public class GraphStatistics
    {
        public int TriplesCount { get; set; }
        public int SubjectsCount { get; set; }
        public int PredicatesCount { get; set; }
        public int ObjectsCount { get; set; }
        public int LiteralsCount { get; set; }
    }
}