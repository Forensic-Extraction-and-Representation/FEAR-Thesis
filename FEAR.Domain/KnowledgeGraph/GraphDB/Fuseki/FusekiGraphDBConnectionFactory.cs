namespace FEAR.Domain.KnowledgeGraph.GraphDB.Fuseki
{
    /// <summary>
    /// Factory for creating <see cref="FusekiGraphDBConnection"/> instances.
    /// Inherits from <see cref="GraphDBConnectionFactory{T}"/> and provides
    /// construction logic specific to Fuseki-backed graph database connections.
    /// </summary>
    public class FusekiGraphDBConnectionFactory : GraphDBConnectionFactory<FusekiGraphDBConnection>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GraphQLDBConnectionFactory"/> class
        /// with the specified configuration.
        /// </summary>
        /// <param name="configuration">The configuration for the Fuseki connection.</param>
        public FusekiGraphDBConnectionFactory(FusekiGraphDBConfiguration configuration) : base(configuration)
        {
        }

        /// <summary>
        /// Constructs a new <see cref="FusekiGraphDBConnection"/> using the provided configuration.
        /// </summary>
        /// <returns>A new <see cref="FusekiGraphDBConnection"/> instance.</returns>
        public override FusekiGraphDBConnection ConstructConnection()
        {
            return new FusekiGraphDBConnection(Configuration);
        }
    }
}
