namespace FEAR.Domain.KnowledgeGraph.GraphDB.Sparql
{
    /// <summary>
    /// Factory for creating <see cref="SparqlGraphDBConnection"/> instances.
    /// Inherits from <see cref="GraphDBConnectionFactory{T}"/> and provides
    /// construction logic specific to Sparql-backed graph database connections.
    /// </summary>
    public class SparqlGraphDBConnectionFactory : GraphDBConnectionFactory<SparqlGraphDBConnection>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GraphQLDBConnectionFactory"/> class
        /// with the specified configuration.
        /// </summary>
        /// <param name="configuration">The configuration for the Sparql connection.</param>
        public SparqlGraphDBConnectionFactory(SparqlGraphDBConfiguration configuration) : base(configuration)
        {
        }

        /// <summary>
        /// Constructs a new <see cref="SparqlGraphDBConnection"/> using the provided configuration.
        /// </summary>
        /// <returns>A new <see cref="SparqlGraphDBConnection"/> instance.</returns>
        public override SparqlGraphDBConnection ConstructConnection()
        {
            return new SparqlGraphDBConnection(Configuration);
        }
    }
}
