namespace FEAR.Domain.KnowledgeGraph.GraphDB
{
    /// <summary>
    /// Abstract factory base class for creating graph database connection instances of type <typeparamref name="T"/>.
    /// Provides configuration management and enforces implementation of a strongly-typed connection constructor.
    /// </summary>
    /// <typeparam name="T">A type that implements <see cref="IGraphDBConnection"/>.</typeparam>
    public abstract class GraphDBConnectionFactory<T> : IGraphDBConnectionFactory<T>
        where T : IGraphDBConnection
    {
        /// <summary>
        /// Gets the configuration used for constructing graph database connections.
        /// </summary>
        public GraphDBConfiguration Configuration { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="GraphDBConnectionFactory{T}"/> class
        /// with the specified configuration.
        /// </summary>
        /// <param name="configuration">The configuration for the graph database connection.</param>
        public GraphDBConnectionFactory(GraphDBConfiguration configuration)
        {
            Configuration = configuration;
        }

        /// <summary>
        /// Constructs a strongly-typed graph database connection instance.
        /// Must be implemented by derived classes.
        /// </summary>
        /// <returns>An instance of <typeparamref name="T"/>.</returns>
        public abstract T ConstructConnection();

        /// <summary>
        /// Constructs a graph database connection as a non-generic object.
        /// Implements the non-generic interface for compatibility.
        /// </summary>
        /// <returns>An object representing the constructed connection.</returns>
        object IGraphDBConnectionFactory.ConstructConnection()
        {
            return ConstructConnection();
        }
    }
}
