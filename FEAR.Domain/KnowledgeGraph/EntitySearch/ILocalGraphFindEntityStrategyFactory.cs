namespace FEAR.Domain.KnowledgeGraph.EntitySearch
{
    /// <summary>
    /// Defines a factory interface for creating <see cref="IFindEntityStrategy"/> instances
    /// specifically for local graph implementations.
    /// Inherits from <see cref="IFindEntityStrategyFactory"/> and is used to distinguish
    /// factories that are intended for use with local (in-memory or on-premises) graphs.
    /// </summary>
    public interface ILocalGraphFindEntityStrategyFactory : IFindEntityStrategyFactory
    {

    }
}
