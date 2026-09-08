namespace FEAR.Domain.KnowledgeGraph.GraphDB.GraphQL
{
    /// <summary>
    /// Factory for creating <see cref="GraphQLDBConnection"/> instances.
    /// Inherits from <see cref="GraphDBConnectionFactory{T}"/> and provides
    /// construction logic specific to Fuseki-backed graph database connections.
    /// </summary>
    public class GraphQLDBConnectionFactory : GraphDBConnectionFactory<GraphQLDBConnection>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GraphQLDBConnectionFactory"/> class
        /// with the specified configuration.
        /// </summary>
        /// <param name="configuration">The configuration for the Fuseki connection.</param>
        public GraphQLDBConnectionFactory(GraphQLDBConfiguration configuration) : base(configuration)
        {
        }

        /// <summary>
        /// Constructs a new <see cref="GraphQLDBConnection"/> using the provided configuration.
        /// </summary>
        /// <returns>A new <see cref="GraphQLDBConnection"/> instance.</returns>
        public override GraphQLDBConnection ConstructConnection()
        {
            return new GraphQLDBConnection(Configuration);
        }
    }
}
