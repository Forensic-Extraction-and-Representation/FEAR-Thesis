using FEAR.Domain.KnowledgeGraph;
using FEAR.Domain.KnowledgeGraph.EntitySearch;
using FEAR.Domain.KnowledgeGraph.GraphCodify;
using FEAR.Domain.KnowledgeGraph.GraphDB;
using FEAR.Domain.KnowledgeGraph.Graphs;
using FEAR.Domain.KnowledgeGraph.GraphUpdate;
using FEAR.Domain.KnowledgeGraph.TypeService;
using VDS.RDF;
using VDS.RDF.Nodes;
using VDS.RDF.Ontology;
using VDS.RDF.Query;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace FEAR.Runtime.KnowledgeGraph
{
    public abstract class BaseRemoteGraph : BaseMaterializedGraph, IRemoteGraph, ILocalGraph
    {
        protected Lazy<LocalGraph> _localWorkingGraph;
        protected LocalGraph LocalWorkingGraph => _localWorkingGraph.Value;

        protected Lazy<IFindEntityStrategy> _localFindEntityStrategy { get; set; }
        protected IFindEntityStrategyFactory LocalFindEntityStrategyFactory { get; }

        protected Lazy<IFindEntityStrategy> _remoteFindEntityStrategy { get; set; }
        protected IFindEntityStrategyFactory RemoteFindEntityStrategyFactory { get; }
        protected FindEntityStrategyFactoryManager FindEntityStrategyFactoryManager { get; }

        protected override IFindEntityStrategy FindEntityStrategy => _remoteFindEntityStrategy.Value;
        protected IFindEntityStrategy LocalFindEntityStrategy => _localFindEntityStrategy.Value;

        public override OntologyGraph Graph => LocalWorkingGraph.Graph;
        public abstract string GraphLanguage { get; }

        public BaseRemoteGraph(ITypeConversionService typeConversionService, IGraphCodifyServiceConfiguration configuration, FindEntityStrategyFactoryManager findEntityStrategyFactoryManager) : base(typeConversionService, configuration)
        {
            FindEntityStrategyFactoryManager = findEntityStrategyFactoryManager;
            RemoteFindEntityStrategyFactory = FindEntityStrategyFactoryManager.GetFactory<IRemoteGraph>();
            LocalFindEntityStrategyFactory = FindEntityStrategyFactoryManager.GetFactory<ILocalGraph>();

            _remoteFindEntityStrategy = new Lazy<IFindEntityStrategy>(() => RemoteFindEntityStrategyFactory.CreateStrategy(this));
            _localFindEntityStrategy = new Lazy<IFindEntityStrategy>(() => LocalFindEntityStrategyFactory.CreateStrategy(this));
            _localWorkingGraph = new Lazy<LocalGraph>(ConstructLocalWorkingGraph);
        }

        /// <summary>
        /// Constructs the local working graph used for caching and local operations.
        /// </summary>
        protected LocalGraph ConstructLocalWorkingGraph()
        {
            var lg = new LocalGraph(FindEntityStrategyFactoryManager, TypeConversionService, Configuration);
            lg.Purpose = "Local working graph for remote graph " + GraphName;
            lg.Description = "Local working graph for remote graph " + GraphName + ". This graph is used to store the local copy of the remote graph for performance optimization.";
            lg.GraphManager = GraphManager;
            lg.Graph.NamespaceMap.Import(GraphManager.OntologyGraph.NamespaceMap);
            return lg;
        }
        public abstract void AssertAndRetractRemote(TriplesSet triplesSet);

        /// <summary>
        /// Finds an entity using the local strategy first, then falls back to the remote strategy if not found.
        /// </summary>
        public override KGResponse<Entity> FindEntity(GraphUpdateContext context)
        {
            // Try to find the entity locally first
            var y = LocalFindEntityStrategy.FindEntity(context);
            if (y.IsSuccess || context.RemoteLoadPerformed)
            {
                return y;
            }

            // If not found locally, mark as remote load performed and try remote
            context.RemoteLoadPerformed = true;
            return FindEntityStrategy.FindEntity(context);
        }

        /// <summary>
        /// Creates an individual in the local working graph (remote creation is not directly supported here).
        /// </summary>
        public override KGResponse<Individual> CreateIndividual(Uri resource, Uri @class)
        {
            // Only create and return in the local copy of the graph
            return LocalWorkingGraph.CreateIndividual(resource, @class);
        }

        /// <summary>
        /// Gets an individual from the local working graph.
        /// </summary>
        public override KGResponse<Individual> GetIndividual(IUriNode node)
        {
            //LoadIndividualFromRemoteGraph(node);
            return LocalWorkingGraph.GetIndividual(node);
        }

        /// <summary>
        /// Finds subjects of a literal value in the local working graph.
        /// </summary>
        public KGResponse<IEnumerable<Individual>> FindSubjectOfLiteral(string literal, string propertyUri)
        {
            return FindSubjectOfLiteral(new StringNode(literal), propertyUri);
        }

        /// <summary>
        /// Finds subjects of a literal in the local working graph.
        /// </summary>
        public KGResponse<IEnumerable<Individual>> FindSubjectOfLiteral(ILiteralNode literalNode, string propertyUri)
        {
            // Check if the materialized graph has the literal loaded already, if not, search the fuseki graph, if nothing, return error
            var baseSearch = LocalWorkingGraph.FindSubjectOfLiteral(literalNode, propertyUri);

            if (baseSearch.IsSuccess && baseSearch.Value.Count() > 0)
            {
                return baseSearch;
            }

            return new KGResponse<IEnumerable<Individual>>().WithError("Subject was not found with specified literal value");
        }

        protected abstract object ComposeFindSubjectQuery(Uri objectTypeUri, Uri typePredicateUri);

        /// <summary>
        /// Finds all subjects of the specified type using a SPARQL query on the remote graph.
        /// </summary>
        public override KGResponse<IEnumerable<Individual>> FindSubjectsOfType(string type)
        {
            // Prepare URIs and nodes for the type and predicate
            var objectTypeUri = GraphManager.GetUriFromPrefixNotation(type).Value;
            var typePredicateUri = GraphManager.GetUriFromPrefixNotation("rdf:#type").Value;
            var objectTypeNode = GraphManager.OntologyGraph.GetUriNode(objectTypeUri);
            var typePredicateNode = GraphManager.OntologyGraph.GetUriNode(typePredicateUri);

            // Build and execute the SPARQL query
            object query = ComposeFindSubjectQuery(objectTypeUri, typePredicateUri);

            var result = ExecuteQuery(query);

            // Parse the result set and return individuals
            if (result is SparqlResultSet)
            {
                var rs = (SparqlResultSet)result;
                if (rs.Count > 0)
                {
                    var individualsResult = rs.Select(t => LocalWorkingGraph.CreateIndividual(new Uri(t["subject"].ToString()), objectTypeUri).Value);
                    return new KGResponse<IEnumerable<Individual>>().WithSuccess(individualsResult);
                }
            }

            return new KGResponse<IEnumerable<Individual>>().WithSuccess(new Individual[] { });
        }

        /// <summary>
        /// Creates a blank node in the local working graph.
        /// </summary>
        public override IBlankNode CreateBlankNode()
        {
            return LocalWorkingGraph.CreateBlankNode();
        }

        /// <summary>
        /// Gets an individual from the local working graph by resource and class.
        /// </summary>
        public override KGResponse<Individual> GetIndividual(Uri resource, Uri @class)
        {
            return LocalWorkingGraph.GetIndividual(resource, @class);
        }

        /// <summary>
        /// Gets or creates a literal node in the local working graph.
        /// </summary>
        public override KGResponse<ILiteralNode> GetOrCreateLiteralNode(string propertyValue, string typeUri)
        {
            return LocalWorkingGraph.GetOrCreateLiteralNode(propertyValue, typeUri);
        }

        /// <summary>
        /// Creates a literal node if not null in the local working graph.
        /// </summary>
        public override KGResponse<ILiteralNode> CreateLiteralNodeIfNotNull(string propertyValue, string dataType)
        {
            return LocalWorkingGraph.CreateLiteralNodeIfNotNull(propertyValue, dataType);
        }
        
        /// <summary>
        /// Adds a resource property to an individual in the local working graph.
        /// </summary>
        public override void AddResourceProperty(Individual individual, Uri propertyUri, INode resource, bool persist)
        {
            LocalWorkingGraph.AddResourceProperty(individual, propertyUri, resource, persist);
        }

        /// <summary>
        /// Adds a literal property to an individual in the local working graph.
        /// </summary>
        public override void AddLiteralProperty(Individual individual, Uri propertyUri, ILiteralNode literalNode, bool persist)
        {
            LocalWorkingGraph.AddLiteralProperty(individual, propertyUri, literalNode, persist);
        }

        /// <summary>
        /// Adds a resource property to an individual in the local working graph using a predicate name.
        /// </summary>
        public override void AddResourceProperty(Individual individual, string predicateName, INode resource, bool persist)
        {
            LocalWorkingGraph.AddResourceProperty(individual, predicateName, resource, persist);
        }

        /// <summary>
        /// Asserts and retracts triples in both the remote and local working graphs.
        /// </summary>
        public override void AssertAndRetract(TriplesSet triplesSet)
        {
            AssertAndRetractRemote(triplesSet);
            LocalWorkingGraph.AssertAndRetract(triplesSet);
        }

    }
}
