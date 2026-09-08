using VDS.RDF;
using Neo4j.Driver;
using System.Text;
using FEAR.Domain.KnowledgeGraph.TypeService;
using FEAR.Domain.KnowledgeGraph.GraphManager;
using System.Runtime.CompilerServices;

namespace FEAR.Domain.KnowledgeGraph.GraphDB.Cypher
{
    public class CypherParameterizedString
    {
        public CypherParameterizedString(string query)
        {
            Query = query;
        }

        public string Query { get; set; }
        public Dictionary<string, object> Parameters { get; set; } = new Dictionary<string, object>();
    }

    /// <summary>
    /// Provides a connection to a Fuseki-backed RDF graph database using dotNetRDF's FusekiConnector.
    /// Implements the <see cref="IGraphDBConnection"/> interface for graph operations such as update, delete, and query.
    /// </summary>
    public class CypherDBConnection : IGraphDBConnection
    {
        // Configuration settings for the Fuseki connection.
        private GraphDBConfiguration configuration;

        // Lazily-initialized FusekiConnector instance for communicating with the Fuseki server.
        private Lazy<IDriver> connection = null;

        /// <summary>
        /// Gets the underlying FusekiConnector instance.
        /// </summary>
        public IDriver Connection => connection.Value;

        public ITypeConversionService TypeConversionService { get; }

        /// <summary>
        /// Gets the configuration used for this connection.
        /// </summary>
        private GraphDBConfiguration Configuration { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="GraphQLDBConnection"/> class.
        /// Sets up the connection and applies authentication if required.
        /// </summary>
        /// <param name="configuration">The configuration for the Fuseki connection.</param>
        public CypherDBConnection(GraphDBConfiguration configuration, ITypeConversionService typeConversionService)
        {
            Configuration = configuration;
            TypeConversionService = typeConversionService;
            connection = new Lazy<IDriver>(() =>
            {
                IDriver conn = null;
                if (Configuration.Authentication)
                {
                    conn = GraphDatabase.Driver(Configuration.Endpoint, AuthTokens.Basic(Configuration.Username, Configuration.Password));
                }
                else
                {
                    conn = GraphDatabase.Driver(Configuration.Endpoint);
                }

                GraphUri = Configuration.GraphUri;

                return conn;
            });
        }

        /// <summary>
        /// Gets a value indicating whether update operations are supported by the connection.
        /// </summary>
        public bool UpdateSupported => true;

        /// <summary>
        /// Gets a value indicating whether delete operations are supported by the connection.
        /// </summary>
        public bool DeleteSupported => true;

        /// <summary>
        /// Gets the URI of the graph this connection is associated with.
        /// </summary>
        public string GraphUri { get; private set; }

        private class SubjectPredicateMapForBlankNode
        {
            public IUriNode Subject { get; set; }
            public IUriNode Predicate { get; set; }
            public IBlankNode BlankNode { get; internal set; }
        }

        private bool IsListRelatedUri(Triple triple)
        {
            if (triple.Subject is IBlankNode)
                return true;
            else
            {
                var uri = triple.Predicate.As<IUriNode>()?.Uri;
                return
                    uri.Fragment.Equals("#nil", StringComparison.OrdinalIgnoreCase) ||
                       uri.Fragment.Equals("#first", StringComparison.OrdinalIgnoreCase) ||
                       uri.Fragment.Equals("#rest", StringComparison.OrdinalIgnoreCase);
            }
        }

        /// <summary>
        /// Updates the specified graph by adding and/or removing triples.
        /// </summary>
        /// <param name="graphUri">The URI of the graph to update.</param>
        /// <param name="additions">Triples to add.</param>
        /// <param name="removals">Triples to remove.</param>
        public void UpdateGraph(string graphUri, IEnumerable<Triple> additions, IEnumerable<Triple> removals)
        {
            Dictionary<Uri, string> existingEntityUriToEntityIdInQuery = new Dictionary<Uri, string>();
            Dictionary<IBlankNode, SubjectPredicateMapForBlankNode> blankNodeToParentSubjectPredicateMap = new Dictionary<IBlankNode, SubjectPredicateMapForBlankNode>();
            int entityId = 0;
            int objectId = 0;
            int collectionId = 0;
            CypherParameterizedString query = new CypherParameterizedString("");
            StringBuilder queryBuilder = new StringBuilder();
            Dictionary<IUriNode, string> uriNodeToEntityName = new Dictionary<IUriNode, string>();
            foreach(var x in additions)
            {
                foreach(var node in new VDS.RDF.INode[] { x.Subject, x.Object })
                {
                    if(node is IUriNode uriNode && !uriNodeToEntityName.ContainsKey(uriNode))
                    {
                        uriNodeToEntityName.Add(uriNode, $"entity{entityId++}");
                    }
                }
            }

            foreach (var triple in additions)
            {
                if (triple.Object is IUriNode && (triple.Object.As<IUriNode>()?.Uri.Fragment.Equals("#nil", StringComparison.OrdinalIgnoreCase) ?? false))
                {
                    continue;
                }

                string entityName = "";
                VDS.RDF.INode tripleSubject = triple.Subject;
                SubjectPredicateMapForBlankNode subjectPredicateMap = null;
                Uri triplePredicate = triple.Predicate.As<IUriNode>().Uri;

                if (IsListRelatedUri(triple))
                {
                    if (tripleSubject is IBlankNode)
                    {
                        var blankNode = triple.Subject.As<IBlankNode>();
                        if (!blankNodeToParentSubjectPredicateMap.ContainsKey(blankNode))
                        {
                            var blankNodeParentTriple = additions.WithObject(tripleSubject).SingleOrDefault();
                            while (blankNodeParentTriple.Subject is IBlankNode)
                            {
                                blankNodeParentTriple = additions.WithObject(blankNodeParentTriple.Subject).SingleOrDefault();
                            }
                            tripleSubject = blankNodeParentTriple?.Subject?.As<IUriNode>() ?? throw new InvalidOperationException("Blank node without a parent subject found in additions.");

                            blankNodeToParentSubjectPredicateMap.Add(blankNode, new SubjectPredicateMapForBlankNode
                            {
                                BlankNode = blankNode,
                                Subject = tripleSubject.As<IUriNode>(),
                                Predicate = blankNodeParentTriple.Predicate.As<IUriNode>()
                            });
                        }

                        tripleSubject = blankNodeToParentSubjectPredicateMap[blankNode].Subject;
                        triplePredicate = blankNodeToParentSubjectPredicateMap[blankNode].Predicate.Uri;
                    }
                    else
                    {
                        var listNodeParentTriple = additions.WithObject(tripleSubject).SingleOrDefault();
                        while (listNodeParentTriple.Subject is IBlankNode)
                        {
                            listNodeParentTriple = additions.WithObject(listNodeParentTriple.Subject).SingleOrDefault();
                        }

                        tripleSubject = listNodeParentTriple?.Subject?.As<IUriNode>() ?? throw new InvalidOperationException("Blank node without a parent subject found in additions.");
                    }
                }
                else if (triple.Object is IBlankNode)
                {
                    blankNodeToParentSubjectPredicateMap.Add(triple.Object.As<IBlankNode>(), new SubjectPredicateMapForBlankNode
                    {
                        BlankNode = triple.Object.As<IBlankNode>(),
                        Subject = triple.Subject.As<IUriNode>(),
                        Predicate = triple.Predicate.As<IUriNode>()
                    });

                    // We don't need to do anything else as this is not a predicate we can add to the graph directly.
                    continue;
                }
                else
                {
                    tripleSubject = triple.Subject.As<IUriNode>();
                }

                IUriNode tripleSubjectUriNode = tripleSubject.As<IUriNode>();

                entityName = AddEntityToQuery(tripleSubjectUriNode);

                string objectName = $"Object{objectId}";
                if (triple.Object is IUriNode)
                {
                    var uriNodeName = AddEntityToQuery(triple.Object.As<IUriNode>());
                    queryBuilder.AppendLine($"MERGE ({entityName})-[:`{triplePredicate}`]->({uriNodeName})");
                }
                else if (triple.Object is ILiteralNode)
                {
                    var ln = triple.Object.As<ILiteralNode>();
                    if (tripleSubject is IBlankNode && blankNodeToParentSubjectPredicateMap.ContainsKey(triple.Subject.As<IBlankNode>()))
                    {
                        queryBuilder.AppendLine($"WITH {entityName}, coalesce({entityName}.`{triplePredicate}`, []) as currentValues{collectionId}");
                        queryBuilder.AppendLine($"WHERE NOT ${objectName} IN currentValues{collectionId}");
                        queryBuilder.AppendLine($"SET {entityName}.`{triplePredicate}` = coalesce({entityName}.`{triplePredicate}`, []) + ${objectName}");
                        collectionId++;
                    }
                    else
                    {
                        queryBuilder.AppendLine($"SET {entityName}.`{triplePredicate}` = ${objectName}");
                    }
                    var objValue = TypeConversionService.ConvertFromString(ln.Value, ln.DataType.ToString(), false);
                    query.Parameters.Add(objectName, objValue);
                }

                objectId++;
            }

            query.Query = queryBuilder.ToString();
            Query(query);

            string AddEntityToQuery(IUriNode entityNode)
            {
                string entityName = "";
                if (existingEntityUriToEntityIdInQuery.ContainsKey(entityNode.Uri))
                {
                    entityName = existingEntityUriToEntityIdInQuery[entityNode.Uri];
                }
                else
                {
                    entityName = uriNodeToEntityName[entityNode];
                    existingEntityUriToEntityIdInQuery.Add(entityNode.Uri, entityName);
                    queryBuilder.AppendLine($"MERGE ({entityName}:Resource {{ uri: \"{entityNode.Uri}\" }})");
                }


                return entityName;
            }
        }

        /// <summary>
        /// Updates the specified graph by adding and/or removing triples (using a Uri overload).
        /// </summary>
        /// <param name="graphUri">The URI of the graph to update.</param>
        /// <param name="additions">Triples to add.</param>
        /// <param name="removals">Triples to remove.</param>
        public void UpdateGraph(Uri graphUri, IEnumerable<Triple> additions, IEnumerable<Triple> removals) => UpdateGraph(graphUri.ToString(), additions, removals);

        /// <summary>
        /// Lists all graph URIs available in the Fuseki store.
        /// </summary>
        /// <returns>An enumerable of graph URIs.</returns>
        public IEnumerable<Uri> ListGraphs() => null;// Connection.ListGraphs();

        /// <summary>
        /// Deletes the specified graph from the Fuseki store.
        /// </summary>
        /// <param name="graphUri">The URI of the graph to delete.</param>
        public void DeleteGraph(Uri graphUri) { } // => Connection.DeleteGraph(graphUri);

        public void Update(string queryString) => Query(new CypherParameterizedString(queryString));

        public EagerResult<IReadOnlyList<IRecord>> Query(CypherParameterizedString queryString)
        {
            if (String.IsNullOrEmpty(queryString.Query))
                return null;

            var query = Connection.ExecutableQuery(queryString.Query);
            try
            {
                if (queryString.Parameters?.Count > 0)
                {
                    query.WithParameters(queryString.Parameters);
                }

                var result = query.ExecuteAsync().GetAwaiter().GetResult();
                return result;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error executing query: {queryString.Query}", ex);
            }
        }
    }
}
