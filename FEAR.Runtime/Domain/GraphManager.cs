using FEAR.Domain.KnowledgeGraph.EntitySearch;
using FEAR.Domain.KnowledgeGraph.GraphCodify;
using FEAR.Domain.KnowledgeGraph.GraphManager;
using FEAR.Domain.KnowledgeGraph.Graphs;
using FEAR.Domain.KnowledgeGraph.TypeService;
using FEAR.Domain.Model;

namespace FEAR.Runtime.Domain
{
    /// <summary>
    /// Concrete implementation of <see cref="BaseGraphManager"/> for managing ontology and materialized graphs,
    /// IRI generation, and namespace handling in the FEAR runtime.
    /// </summary>
    public class GraphManager : BaseGraphManager
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GraphManager"/> class.
        /// Passes all configuration and service dependencies to the base class.
        /// </summary>
        /// <param name="config">Graph manager configuration.</param>
        /// <param name="graphUri">Namespace and abbreviation info for the graph.</param>
        /// <param name="graphCodifyServiceConfiguration">Codification service configuration.</param>
        /// <param name="findEntityStrategyFactoryManager">Entity search strategy factory manager.</param>
        /// <param name="typeConversionService">Type conversion service.</param>
        /// <param name="materialized">Materialized graph instance.</param>
        public GraphManager(
            GraphManagerConfiguration config,
            IGraphUriInfo graphUri,
            IGraphCodifyServiceConfiguration graphCodifyServiceConfiguration,
            FindEntityStrategyFactoryManager findEntityStrategyFactoryManager,
            ITypeConversionService typeConversionService, 
            IMaterializedGraph materialized)
            : base(config, graphUri, graphCodifyServiceConfiguration, findEntityStrategyFactoryManager, typeConversionService, materialized)
        {
        }
    }
}
