using FEAR.Domain.KnowledgeGraph.TypeService;

namespace FEAR.Domain.KnowledgeGraph.GraphDB.Cypher
{
    /// <summary>
    /// Factory for creating <see cref="GraphQLDBConnection"/> instances.
    /// Inherits from <see cref="GraphDBConnectionFactory{T}"/> and provides
    /// construction logic specific to Fuseki-backed graph database connections.
    /// </summary>
    public class CypherDBConnectionFactory : GraphDBConnectionFactory<CypherDBConnection>
    {
        public ITypeConversionService TypeConversionService { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="GraphQLDBConnectionFactory"/> class
        /// with the specified configuration.
        /// </summary>
        /// <param name="configuration">The configuration for the Fuseki connection.</param>
        public CypherDBConnectionFactory(CypherDBConfiguration configuration, ITypeConversionService typeConversionService) : base(configuration)
        {
            TypeConversionService = typeConversionService;
        }

        /// <summary>
        /// Constructs a new <see cref="GraphQLDBConnection"/> using the provided configuration.
        /// </summary>
        /// <returns>A new <see cref="GraphQLDBConnection"/> instance.</returns>
        public override CypherDBConnection ConstructConnection()
        {
            return new CypherDBConnection(Configuration, TypeConversionService);
        }
    }
}
