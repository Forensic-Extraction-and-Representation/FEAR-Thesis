namespace FEAR.Domain.KnowledgeGraph.Graphs
{
    /// <summary>
    /// Represents a temporary or short-lived graph instance within the knowledge graph system.
    /// It is intended for ephemeral (non-persistent) use cases such as intermediate
    /// representations during the graph codification process.
    /// </summary>
    public interface IEphemeralGraph : IRealGraph
    { }
}
