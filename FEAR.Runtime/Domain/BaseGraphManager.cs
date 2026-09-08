using FEAR.Domain.Helpers;
using FEAR.Domain.KnowledgeGraph;
using FEAR.Domain.KnowledgeGraph.EntitySearch;
using FEAR.Domain.KnowledgeGraph.GraphCodify;
using FEAR.Domain.KnowledgeGraph.GraphManager;
using FEAR.Domain.KnowledgeGraph.Graphs;
using FEAR.Domain.KnowledgeGraph.GraphUpdate;
using FEAR.Domain.KnowledgeGraph.IRI;
using FEAR.Domain.KnowledgeGraph.TypeService;
using FEAR.Domain.Model;
using FEAR.Runtime.KnowledgeGraph;
using System.Collections.Concurrent;
using VDS.RDF;
using VDS.RDF.Ontology;

namespace FEAR.Runtime.Domain
{
    /// <summary>
    /// Abstract base class for managing ontology and materialized graphs, IRI generation, and namespace handling.
    /// Provides core logic for graph initialization, namespace management, and utility methods for derived graph managers.
    /// </summary>
    public abstract class BaseGraphManager : UriBasedGraph, IGraphManager
    {
        /// <summary>
        /// Gets the ontology graph, which contains schema and vocabulary definitions.
        /// </summary>
        public OntologyGraph OntologyGraph { get; }

        /// <summary>
        /// Gets the materialized graph, which contains the final graph of all data and relationships.
        /// </summary>
        public IMaterializedGraph MaterializedGraph { get; }

        /// <summary>
        /// Gets the IRI generator used for creating unique IRIs for entities.
        /// </summary>
        public IIRIGenerator IRIGenerator { get; }

        /// <summary>
        /// Gets the factory manager for entity search strategies.
        /// </summary>
        public FindEntityStrategyFactoryManager FindEntityStrategyFactoryManager { get; }

        /// <summary>
        /// Gets the type conversion service for .NET and RDF/XSD types.
        /// </summary>
        public ITypeConversionService TypeConversionService { get; }

        /// <summary>
        /// Gets the configuration for the graph codification service.
        /// </summary>
        public IGraphCodifyServiceConfiguration GraphCodifyServiceConfiguration { get; }

        /// <summary>
        /// Not implemented: Gets the underlying ontology graph instance.
        /// </summary>
        public override OntologyGraph Graph => throw new NotImplementedException();

        /// <summary>
        /// Not implemented: Gets the entity search strategy for this graph.
        /// </summary>
        protected override IFindEntityStrategy FindEntityStrategy => throw new NotFiniteNumberException();

        /// <summary>
        /// Cache for resolved prefix notations to URIs.
        /// </summary>
        private readonly ConcurrentDictionary<String, Uri> prefixCache = new ConcurrentDictionary<String, Uri>();

        /// <summary>
        /// Initializes a new instance of the <see cref="BaseGraphManager"/> class.
        /// Sets up ontology and materialized graphs, namespace mappings, and loads ontology files if present.
        /// </summary>
        /// <param name="config">Graph manager configuration.</param>
        /// <param name="graphUri">Namespace and abbreviation info for the graph.</param>
        /// <param name="graphCodifyServiceConfiguration">Codification service configuration.</param>
        /// <param name="findEntityStrategyFactoryManager">Entity search strategy factory manager.</param>
        /// <param name="typeConversionService">Type conversion service.</param>
        /// <param name="materialized">Materialized graph instance.</param>
        public BaseGraphManager(
            GraphManagerConfiguration config,
            IGraphUriInfo graphUri,
            IGraphCodifyServiceConfiguration graphCodifyServiceConfiguration,
            FindEntityStrategyFactoryManager findEntityStrategyFactoryManager,
            ITypeConversionService typeConversionService,
            IMaterializedGraph materialized)
        {
            OntologyGraph = new OntologyGraph();
            MaterializedGraph = materialized;

            // Link the materialized graph back to this manager
            MaterializedGraph.GraphManager = this;

            GraphCodifyServiceConfiguration = graphCodifyServiceConfiguration;
            TypeConversionService = typeConversionService;
            FindEntityStrategyFactoryManager = findEntityStrategyFactoryManager;
            IRIGenerator = config.IRIGeneratorFunc(graphUri.NamespaceAbbrev);

            // Add namespace mappings to both ontology and materialized graphs
            OntologyGraph.NamespaceMap.AddNamespace(graphUri.NamespaceAbbrev, new Uri(graphUri.Namespace));
            MaterializedGraph.Graph.NamespaceMap.AddNamespace(graphUri.NamespaceAbbrev, new Uri(graphUri.Namespace));

            // Load and merge ontology files if the folder exists
            if (Directory.Exists(config.OntologyGraphFolder))
            {
                foreach (var file in Directory.GetFiles(config.OntologyGraphFolder))
                {
                    var _og = new OntologyGraph();
                    _og.LoadFromFile(file);
                    OntologyGraph.Merge(_og, false);
                    if (config.IncludeOntologyInMaterialisedGraph)
                    {
                        MaterializedGraph.Graph.Merge(_og, false);
                    }
                }

                // Import namespace mappings if ontology is not included in the materialized graph
                if (!config.IncludeOntologyInMaterialisedGraph)
                {
                    MaterializedGraph.Graph.NamespaceMap.Import(OntologyGraph.NamespaceMap);
                }
            }
        }

        /// <summary>
        /// Not implemented: Finds an entity in the graph using the provided update context.
        /// </summary>
        /// <param name="entityContext">The context for the entity search.</param>
        /// <returns>A response containing the found entity or error information.</returns>
        public override KGResponse<Entity> FindEntity(GraphUpdateContext entityContext)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Creates a temporary local graph for a specific purpose and description.
        /// </summary>
        /// <param name="purpose">The purpose of the temporary graph.</param>
        /// <param name="description">A description of the temporary graph.</param>
        /// <returns>A new <see cref="ILocalGraph"/> instance.</returns>
        public ILocalGraph CreateTemporaryGraph(string purpose, string description)
        {
            var lg = new LocalGraph(FindEntityStrategyFactoryManager, TypeConversionService, GraphCodifyServiceConfiguration);
            lg.Graph.NamespaceMap.Import(OntologyGraph.NamespaceMap);
            lg.GraphManager = this;
            lg.Purpose = purpose;
            lg.Description = description;

            return lg;
        }

        /// <summary>
        /// Adds a namespace mapping to the ontology and materialized graphs.
        /// </summary>
        /// <param name="prefix">The namespace prefix.</param>
        /// <param name="uri">The namespace URI.</param>
        public void AddNamespace(string prefix, Uri uri)
        {
            Uri fullUri = uri;
            // Ensure the URI has an authority; if not, construct one from the namespace map
            while (String.IsNullOrEmpty(fullUri.Authority))
            {
                fullUri = new Uri(OntologyGraph.NamespaceMap.GetNamespaceUri(fullUri.Scheme), fullUri.Segments[0] + "/");
            }

            if (!OntologyGraph.NamespaceMap.Prefixes.Contains(prefix))
            {
                OntologyGraph.NamespaceMap.AddNamespace(prefix, fullUri);
                MaterializedGraph.Graph.NamespaceMap.Import(OntologyGraph.NamespaceMap);
            }
        }

        /// <summary>
        /// Creates a new graph codification service for entity and relationship management.
        /// </summary>
        /// <returns>An <see cref="IGraphCodifyService"/> instance.</returns>
        public IGraphCodifyService CreateGraphCodifyService()
        {
            return new GraphCodifyService(FindEntityStrategyFactoryManager, GraphCodifyServiceConfiguration, this, TypeConversionService);
        }

        /// <summary>
        /// Creates a new IRI for an entity, using the specified type and URI segment.
        /// </summary>
        /// <param name="type">The type of entity (used in the IRI if no segment is provided).</param>
        /// <param name="uriSegment">A custom segment for the IRI.</param>
        /// <returns>A new <see cref="Uri"/> representing the entity's IRI.</returns>
        public Uri CreateIRI(string type, string uriSegment)
        {
            var newIRI = IRIGenerator.CreateIRI(type, uriSegment);
            return GetUriFromPrefixNotation(newIRI.ToString()).Value;
        }

        /// <summary>
        /// Resolves a URI from a prefix notation string (e.g., "rdf:type").
        /// Caches results for performance.
        /// </summary>
        /// <param name="prefixNotation">The prefixed string to resolve.</param>
        /// <returns>A <see cref="KGResponse{Uri}"/> containing the resolved URI or error information.</returns>
        public KGResponse<Uri> GetUriFromPrefixNotation(string prefixNotation)
        {
            if (prefixCache.TryGetValue(prefixNotation, out Uri cachedUri))
                return new KGResponse<Uri>().WithSuccess(cachedUri);
            else
            {
                var regexMatch = UriBasedGraphHelpers.PrefixNotationRegex.Match(prefixNotation);
                if (regexMatch.Success)
                {
                    if (regexMatch.Captures.Count == 1 && regexMatch.Groups.Count == 3)
                    {
                        var prefix = regexMatch.Groups[1].Value;
                        var className = regexMatch.Groups[2].Value;

                        foreach (var nsMapper in new INamespaceMapper[] { OntologyGraph.NamespaceMap, MaterializedGraph.Graph.NamespaceMap })
                        {
                            if (nsMapper.HasNamespace(prefix))
                            {
                                var prefixUri = nsMapper.GetNamespaceUri(prefix);

                                prefixCache.TryAdd(prefixNotation, new Uri(prefixUri.ToString() + className));
                                return new KGResponse<Uri>().WithSuccess(new Uri(prefixUri.ToString() + className));
                            }
                        }
                    }
                }
                else
                {
                    if (Uri.TryCreate(prefixNotation, UriKind.Absolute, out Uri uri) && !string.IsNullOrEmpty(uri.Authority))
                    {
                        prefixCache.TryAdd(prefixNotation, uri);
                        return new KGResponse<Uri>().WithSuccess(uri);
                    }
                }

                return new KGResponse<Uri>().WithError("Prefix was not found in the namespace mappings.");
            }   
        }
    }
}
