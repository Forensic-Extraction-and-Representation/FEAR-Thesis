namespace FEAR.Domain.KnowledgeGraph.Graphs
{
    /// <summary>
    /// Represents a local, in-memory graph that may be persistent or temporary.
    /// </summary>
    public interface ILocalGraph : IMaterializedGraph, IEphemeralGraph
    {
    }
}
