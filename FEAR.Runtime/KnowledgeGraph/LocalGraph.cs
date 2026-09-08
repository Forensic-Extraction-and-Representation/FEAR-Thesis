using FEAR.Domain.KnowledgeGraph;
using FEAR.Domain.KnowledgeGraph.Collections;
using FEAR.Domain.KnowledgeGraph.EntitySearch;
using FEAR.Domain.KnowledgeGraph.GraphCodify;
using FEAR.Domain.KnowledgeGraph.Graphs;
using FEAR.Domain.KnowledgeGraph.GraphUpdate;
using FEAR.Domain.KnowledgeGraph.TypeService;
using FEAR.Runtime.KnowledgeGraph.Sparql.Collections;
using VDS.RDF;
using VDS.RDF.Nodes;
using VDS.RDF.Ontology;
using VDS.RDF.Parsing;
using VDS.RDF.Query;
using VDS.RDF.Query.Datasets;
using VDS.RDF.Update;

namespace FEAR.Runtime.KnowledgeGraph
{
    /// <summary>
    /// Implements a local, in-memory materialized knowledge graph for FEAR.
    ///
    /// <para>
    /// <b>LocalGraph</b> is used to store, query, and update entities, individuals, and relationships
    /// in a local (non-remote) RDF/OWL graph. It supports all core graph codification operations, including
    /// entity search, property assertion, SPARQL updates, and collection management.
    /// </para>
    /// <para>
    /// In the FEAR pipeline, this class is responsible for processing and materializing data that is queued
    /// for graph codify scripts—whether that data is an artifact directly or the result of a collector (CFEAR) script.
    /// </para>
    /// </summary>
    public class LocalGraph : BaseMaterializedGraph, ILocalGraph
    {
        /// <summary>
        /// Lazily-initialized entity search strategy for this graph.
        /// </summary>
        protected Lazy<IFindEntityStrategy> _findEntityStrategy { get; set; }

        /// <summary>
        /// The factory used to create entity search strategies for this graph.
        /// </summary>
        protected IFindEntityStrategyFactory FindEntityStrategyFactory { get; }

        /// <summary>
        /// Gets the entity search strategy for this graph.
        /// </summary>
        protected override IFindEntityStrategy FindEntityStrategy => _findEntityStrategy.Value;

        /// <summary>
        /// Gets the name of this graph ("LOCAL").
        /// </summary>
        public override string GraphName => "LOCAL";

        /// <summary>
        /// Lazily-initialized ontology graph instance.
        /// </summary>
        private Lazy<OntologyGraph> _graph = new Lazy<OntologyGraph>(() => new OntologyGraph());

        /// <summary>
        /// Gets the underlying ontology graph.
        /// </summary>
        public override OntologyGraph Graph => _graph.Value;

        /// <summary>
        /// Initializes a new instance of the <see cref="LocalGraph"/> class.
        /// </summary>
        /// <param name="findEntityStrategyFactoryManager">The manager for entity search strategy factories.</param>
        /// <param name="typeConversionService">The type conversion service.</param>
        /// <param name="configuration">The graph codify service configuration.</param>
        public LocalGraph(FindEntityStrategyFactoryManager findEntityStrategyFactoryManager, ITypeConversionService typeConversionService,
            IGraphCodifyServiceConfiguration configuration)
            : base(typeConversionService, configuration)
        {
            FindEntityStrategyFactory = findEntityStrategyFactoryManager.GetFactory<LocalGraph>();
            _findEntityStrategy = new Lazy<IFindEntityStrategy>(() => FindEntityStrategyFactory.CreateStrategy(this));
        }

        /// <summary>
        /// Gets or creates an individual in the graph by resource and class URI.
        /// </summary>
        public override KGResponse<Individual> GetIndividual(Uri resource, Uri @class)
        {
            return CreateIndividual(resource, @class);
        }

        /// <summary>
        /// Finds an entity in the graph using the configured entity search strategy.
        /// </summary>
        public override KGResponse<Entity> FindEntity(GraphUpdateContext entityContext)
        {
            return FindEntityStrategy.FindEntity(entityContext);
        }

        /// <summary>
        /// Gets an individual in the graph by URI node.
        /// </summary>
        public override KGResponse<Individual> GetIndividual(IUriNode node)
        {
            KGResponse<Individual> response;
            var existingNode = Graph.GetUriNode((node as UriNode).Uri);
            try
            {
                Individual individual = existingNode != null ? Graph.CreateIndividual(existingNode) : null;
                return new KGResponse<Individual>().WithErrorIfNull(individual, "Individual node not found");
            }
            catch (Exception ex)
            {
                response = new KGResponse<Individual>().WithError(ex);
            }

            return response;
        }

        /// <summary>
        /// Gets or creates a literal node in the graph for the given value and type.
        /// </summary>
        public override KGResponse<ILiteralNode> GetOrCreateLiteralNode(string propertyValue, string typeUriStr)
        {
            var typeUri = GraphManager.GetUriFromPrefixNotation(typeUriStr).Value;
            var literalNode = Graph.GetLiteralNode(propertyValue, typeUri) ?? Graph.CreateLiteralNode(propertyValue, typeUri);

            return new KGResponse<ILiteralNode>().WithSuccess(literalNode);
        }

        /// <summary>
        /// Creates a literal node if the value is not null; otherwise, returns a successful response with null.
        /// </summary>
        public override KGResponse<ILiteralNode> CreateLiteralNodeIfNotNull(string propertyValue, string dataType)
        {
            return propertyValue != null ? GetOrCreateLiteralNode(propertyValue, dataType) : new KGResponse<ILiteralNode>().WithSuccess(null);
        }

        /// <summary>
        /// Finds all subjects of the specified type in the graph.
        /// </summary>
        public override KGResponse<IEnumerable<Individual>> FindSubjectsOfType(string type)
        {
            var objectTypeUri = GraphManager.GetUriFromPrefixNotation(type).Value;
            var typePredicateUri = GraphManager.GetUriFromPrefixNotation("rdf:type").Value;
            var objectTypeNode = GraphManager.GetNodeForUri(objectTypeUri);
            var typePredicateNode = GraphManager.GetNodeForUri(typePredicateUri);
            var triplesWithPredicate = Graph.Triples.WithPredicateObject(typePredicateNode, objectTypeNode);

            return new KGResponse<IEnumerable<Individual>>().WithSuccess(triplesWithPredicate.Select(t => GetIndividual(t.Subject as IUriNode).Value));
        }

        /// <summary>
        /// Adds a resource property to an individual in the graph.
        /// </summary>
        public override void AddResourceProperty(Individual individual, Uri propertyUri, INode resource, bool persist)
        {
            individual.AddResourceProperty(propertyUri, resource, persist);
        }

        /// <summary>
        /// Adds a literal property to an individual in the graph.
        /// </summary>
        public override void AddLiteralProperty(Individual individual, Uri propertyUri, ILiteralNode literalNode, bool persist)
        {
            individual.AddLiteralProperty(propertyUri, literalNode, persist);
        }

        /// <summary>
        /// Adds a resource property to an individual using a predicate name.
        /// </summary>
        public override void AddResourceProperty(Individual individual, string predicateName, INode resource, bool persist)
        {
            individual.AddResourceProperty(predicateName, resource, persist);
        }

        /// <summary>
        /// Creates a blank node in the graph.
        /// </summary>
        public override IBlankNode CreateBlankNode()
        {
            return Graph.CreateBlankNode();
        }

        /// <summary>
        /// Executes a collection operation result (typically a SPARQL update).
        /// </summary>
        public override void ExecuteCollectionResult(CollectionOperationResult cor)
        {
            SparqlParameterizedString sparqlParameterizedString = new SparqlParameterizedString(cor.Query);
            if (cor.QueryParameters.Count > 0)
            {
                foreach (var param in cor.QueryParameters)
                {
                    sparqlParameterizedString.SetParameter(param.Key, (INode)param.Value);
                }
            }

            ExecuteUpdate(sparqlParameterizedString);
        }

        /// <summary>
        /// Asserts and retracts triples in the graph as specified by the given set.
        /// </summary>
        public override void AssertAndRetract(TriplesSet triplesSet)
        {
            Graph.Assert(triplesSet.Assert);
            Graph.Retract(triplesSet.Retract);
        }

        /// <summary>
        /// Clears all triples from the graph.
        /// </summary>
        public override void Clear()
        {
            Graph.Clear();
        }

        /// <summary>
        /// Gets the total number of triples in the graph.
        /// </summary>
        public override int TriplesCount()
        {
            return Graph.Triples.Count;
        }

        /// <summary>
        /// Executes a SPARQL update query against the graph.
        /// </summary>
        public override void ExecuteUpdate<T>(T query)
        {
            if (query is SparqlParameterizedString sparqlQuery)
            {
                InMemoryDataset data = new InMemoryDataset(Graph);
                LeviathanUpdateProcessor leviathanUpdateProcessor = new LeviathanUpdateProcessor(data, delegate (LeviathanUpdateOptions options)
                {
                    options.UriFactory = new CachingUriFactory(Graph.UriFactory);
                });
                SparqlUpdateParser sparqlUpdateParser = new SparqlUpdateParser();
                SparqlUpdateCommandSet commandSet = sparqlUpdateParser.ParseFromString(sparqlQuery);
                leviathanUpdateProcessor.ProcessCommandSet(commandSet);
            }
            else
            {
                throw new ArgumentException("Unsupported query type for update execution. Expected SparqlParameterizedString.");
            }
        }

        /// <summary>
        /// Executes a SPARQL query against the graph.
        /// </summary>
        public virtual object ExecuteQuery(SparqlQuery query)
        {
            return Graph.ExecuteQuery(query);
        }

        /// <summary>
        /// Executes a SPARQL query (parameterized) against the graph.
        /// </summary>
        public override object ExecuteQuery<T>(T query)
        {
            if (query is SparqlParameterizedString sparqlQuery)
            {
                return Graph.ExecuteQuery(sparqlQuery);
            }
            else if (query is SparqlQuery sparqlQueryObj)
            {
                return Graph.ExecuteQuery(sparqlQueryObj);
            }
            else
            {
                throw new ArgumentException("Unsupported query type");
            }
        }

        public override ICollectionOperationStrategy GetCollectionCreationStrategy(GraphUpdateContext context)
        {
            return new CollectionCreationStrategy(context);
        }

        public override ICollectionOperationStrategy GetCollectionUpdateStrategy(GraphUpdateContext context)
        {
            return new CollectionUpdaterStrategy(context);
        }

        public override GraphStatistics GetStatistics()
        {
            var allTripes = Graph.Triples.ToList();
            GraphStatistics gs = new GraphStatistics()
            {
                LiteralsCount = allTripes.OfType<Triple>().Where(t => t.Object.NodeType == NodeType.Literal).Select(t => (t.Object as ILiteralNode).Value).Distinct().Count(),
                ObjectsCount = allTripes.OfType<Triple>().Where(t => t.Object.NodeType == NodeType.Uri).Select(t => (t.Object as IUriNode).Uri).Distinct().Count(),
                PredicatesCount = allTripes.OfType<Triple>().Where(t => t.Predicate.NodeType == NodeType.Uri).Select(t => (t.Predicate as IUriNode).Uri).Distinct().Count(),
                SubjectsCount = allTripes.OfType<Triple>().Where(t => t.Subject.NodeType == NodeType.Uri).Select(t => (t.Subject as IUriNode).Uri).Distinct().Count(),
                TriplesCount = allTripes.Count
            };

            return gs;
        }
    }
}
