using FEAR.Domain.KnowledgeGraph.Graphs;

namespace FEAR.Domain.KnowledgeGraph.EntitySearch
{
    /// <summary>
    /// Manages the selection of the appropriate <see cref="IFindEntityStrategyFactory"/> implementation
    /// for local and remote graph types. Ensures that both local and remote factories are available.
    /// </summary>
    public class FindEntityStrategyFactoryManager
    {
        /// <summary>
        /// Factory for creating entity search strategies for local graphs.
        /// </summary>
        IFindEntityStrategyFactory LocalFindFactory { get; set; }

        /// <summary>
        /// Factory for creating entity search strategies for remote graphs.
        /// </summary>
        IFindEntityStrategyFactory RemoteFindFactory { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="FindEntityStrategyFactoryManager"/> class.
        /// Selects the appropriate factories for local and remote graphs from the provided collection.
        /// </summary>
        /// <param name="strategies">A collection of available <see cref="IFindEntityStrategyFactory"/> implementations.</param>
        /// <exception cref="Exception">Thrown if a required factory for local or remote graphs is not found.</exception>
        public FindEntityStrategyFactoryManager(IEnumerable<IFindEntityStrategyFactory> strategies)
        {
            LocalFindFactory = strategies.SingleOrDefault(s => s is ILocalGraphFindEntityStrategyFactory);
            RemoteFindFactory = strategies.SingleOrDefault(s => s is IRemoteGraphFindEntityStrategyFactory);

            if (LocalFindFactory == null)
                throw new Exception("Could not locate a IFindEntityStrategyFactory for local graphs");
            if (RemoteFindFactory == null)
            {
                // We don't throw an exception as remote graphs are not always required.
                //throw new Exception("Could not locate a IFindEntityStrategyFactory for remote graphs");
            }
        }

        /// <summary>
        /// Gets the appropriate factory for the specified graph type parameter.
        /// </summary>
        /// <typeparam name="T">The graph type.</typeparam>
        /// <param name="graph">The graph instance.</param>
        /// <returns>The matching <see cref="IFindEntityStrategyFactory"/>.</returns>
        public IFindEntityStrategyFactory GetFactory<T>()
        {
            if (typeof(T) == typeof(IRemoteGraph))
            {
                return RemoteFindFactory;
            }

            return LocalFindFactory;
        }
    }
}
