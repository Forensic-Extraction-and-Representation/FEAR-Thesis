namespace FEAR.Domain.KnowledgeGraph.GraphDB
{
    /// <summary>
    /// Defines a factory interface for creating graph database connection instances.
    /// Provides a non-generic method for constructing a connection as an object.
    /// </summary>
    public interface IGraphDBConnectionFactory
    {
        /// <summary>
        /// Constructs a new graph database connection instance.
        /// </summary>
        /// <returns>The constructed connection as an object.</returns>
        object ConstructConnection();
    }

    /// <summary>
    /// Defines a generic factory interface for creating strongly-typed graph database connection instances.
    /// Inherits from <see cref="IGraphDBConnectionFactory"/> and provides a type-safe method for constructing connections.
    /// </summary>
    /// <typeparam name="T">A type that implements <see cref="IGraphDBConnection"/>.</typeparam>
    public interface IGraphDBConnectionFactory<T> : IGraphDBConnectionFactory
        where T : IGraphDBConnection
    {
        /// <summary>
        /// Constructs a new strongly-typed graph database connection instance.
        /// </summary>
        /// <returns>The constructed connection of type <typeparamref name="T"/>.</returns>
        new T ConstructConnection();
    }
}
