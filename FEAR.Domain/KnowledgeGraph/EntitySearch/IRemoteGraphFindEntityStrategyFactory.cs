namespace FEAR.Domain.KnowledgeGraph.EntitySearch
{
    /// <summary>
    /// Defines a factory interface for creating <see cref="IFindEntityStrategy"/> instances
    /// specifically for remote graph implementations.
    /// Inherits from <see cref="IFindEntityStrategyFactory"/> and is used to distinguish
    /// factories that are intended for use with remote (external or network-based) graphs.
    /// </summary>
    public interface IRemoteGraphFindEntityStrategyFactory : IFindEntityStrategyFactory
    {

    }
}
