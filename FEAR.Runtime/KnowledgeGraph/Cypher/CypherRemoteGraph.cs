using FEAR.Domain.KnowledgeGraph;
using FEAR.Domain.KnowledgeGraph.Collections;
using FEAR.Domain.KnowledgeGraph.EntitySearch;
using FEAR.Domain.KnowledgeGraph.GraphCodify;
using FEAR.Domain.KnowledgeGraph.GraphDB.Cypher;
using FEAR.Domain.KnowledgeGraph.Graphs;
using FEAR.Domain.KnowledgeGraph.GraphUpdate;
using FEAR.Domain.KnowledgeGraph.TypeService;
using FEAR.Runtime.Helpers;
using FEAR.Runtime.KnowledgeGraph.Cypher.Collections;
using System.ComponentModel;
using VDS.RDF;
using VDS.RDF.Nodes;
using VDS.RDF.Ontology;
using VDS.RDF.Query;

namespace FEAR.Runtime.KnowledgeGraph.Cypher
{
    public class CypherRemoteGraph : BaseRemoteGraph, IRemoteGraph<CypherParameterizedString>
    {
        private CypherDBConnectionFactory ConnectionFactory { get; set; }
        public CypherDBConnection Connection { get; private set; }

        public override string GraphLanguage => "Cypher";

        protected IFindEntityStrategy LocalFindEntityStrategy => _localFindEntityStrategy.Value;

        // Implementation for Cypher remote graph would go here
        // This is a placeholder for the actual implementation
        public CypherRemoteGraph(CypherDBConnectionFactory connectionFactory, ITypeConversionService typeConversionService,
            IGraphCodifyServiceConfiguration configuration, FindEntityStrategyFactoryManager findEntityStrategyFactoryManager)
            : base(typeConversionService, configuration, findEntityStrategyFactoryManager)
        {
            ConnectionFactory = connectionFactory;
            Connection = ConnectionFactory.ConstructConnection();
        }

        public override string GraphName => Connection.GraphUri;

        public override void Clear()
        {
            // Delete all triples from the remote graph
            ExecuteUpdate(new CypherParameterizedString("MATCH (n)\r\nDETACH DELETE n"));
            // Re-initialize the local working graph
            _localWorkingGraph = new Lazy<LocalGraph>(ConstructLocalWorkingGraph);
            LocalWorkingGraph.Clear();
        }

        public void ExecuteConstructQuery(CypherParameterizedString query)
        {
            ExecuteConstructQuery(query, LocalWorkingGraph);
        }

        public void ExecuteConstructQuery(CypherParameterizedString query, IRealGraph localGraph)
        {
            var result = ExecuteQuery(query);
            if (result is Graph g)
            {
                string s = g.SerializeTriples();
                foreach (var triple in g.Triples)
                {
                    // Assert each triple into the local graph
                    localGraph.Graph.Assert(triple);
                }
            }
        }

        Dictionary<Type, TypeConverter> _typeConverters = new Dictionary<Type, TypeConverter>();

        public override object ExecuteQuery<T>(T query)
        {
            if (query is not CypherParameterizedString && query is not string)
            {
                throw new ArgumentException("Query must be of type CypherParameterizedString", nameof(query));
            }

            CypherParameterizedString parameterizedString = null;
            if (query is string)
            {
                parameterizedString = new CypherParameterizedString(query as string);
            }
            else
            {
                parameterizedString = query as CypherParameterizedString;
            }

            var queryResult = Connection.Query(parameterizedString);

            if (queryResult != null)
            {
                TripleKeys canCreateTriples = CanKeysBeUsedAsTriples(queryResult.Keys);

                if (canCreateTriples.CanCreateTriples)
                {
                    IGraph resultGraph = new Graph();
                    foreach (var record in queryResult.Result)
                    {
                        var triple = VariableBoundResultToTriple(record, canCreateTriples.SubjectKey, canCreateTriples.PredicateKey, canCreateTriples.ObjectKey, record[canCreateTriples.RelationTypeKey].ToString());

                        resultGraph.Assert(triple);
                    }

                    return resultGraph;
                }
                else
                {
                    // Check if we can create triples from the columns
                    List<String> entitiesToLoad = new List<string>();
                    foreach (var vName in queryResult.Result[0].Keys)
                    {
                        if (vName.EndsWith("_uri"))
                        {
                            string entityName = vName.Substring(0, vName.Length - 4);

                            if (queryResult.Result[0].Keys.Contains(entityName + "_props") && queryResult.Result[0].Keys.Contains(entityName + "_rels"))
                                entitiesToLoad.Add(entityName);
                        }
                    }

                    if (queryResult.Result[0].Keys.Count == entitiesToLoad.Count * 3)
                    {
                        IGraph resultGraph = new Graph();

                        foreach (var entityName in entitiesToLoad)
                        {
                            var entityUriString = queryResult.Result[0][entityName + "_uri"] as string;
                            var entityUriNode = new UriNode(new Uri(entityUriString));
                            var propertyDictionary = queryResult.Result[0][entityName + "_props"] as Dictionary<string, object>;
                            var relationList = queryResult.Result[0][entityName + "_rels"] as IEnumerable<object>;
                            // Load properties and relationships into the local graph
                            foreach (var prop in propertyDictionary)
                            {
                                if (prop.Key == "uri")
                                    continue;

                                var objTypeUri = TypeConversionService.GetTypeUriForObject(prop.Value);

                                var obj = new LiteralNode(TypeConversionService.ConvertToString(prop.Value, objTypeUri.ToString(), false), objTypeUri);
                                resultGraph.Assert(new Triple(entityUriNode, new UriNode(new Uri(prop.Key)), obj));
                            }

                            foreach (var rel in relationList)
                            {
                                if (rel is List<object> relations)
                                {
                                    if (Uri.TryCreate(relations[1].ToString(), UriKind.Absolute, out Uri uri))
                                    {
                                        var relationUriNode = new UriNode(uri);
                                        resultGraph.Assert(new Triple(entityUriNode, new UriNode(new Uri(relations[0].ToString())), relationUriNode));
                                    }
                                }
                            }
                        }

                        return resultGraph;
                    }
                    else
                    {
                        IEnumerable<KeyValuePair<string, INode>> variableMap = new List<KeyValuePair<string, INode>>();
                        foreach (var record in queryResult.Result)
                        {
                            foreach (var key in record.Keys)
                            {
                                INode node = null;
                                var objEntry = record[key];
                                if (Uri.TryCreate(objEntry?.ToString(), UriKind.Absolute, out Uri uri))
                                {
                                    node = new UriNode(uri);
                                }
                                else
                                {
                                    var objTypeUri = TypeConversionService.GetTypeUriForObject(objEntry);
                                    var objEntryType = objEntry.GetType();

                                    TypeConverter converter;
                                    if (!_typeConverters.TryGetValue(objEntryType, out converter))
                                    {
                                        converter = TypeConversionService.GetConverterForTypeUri(TypeConversionService.GetTypeUriForObject(objEntry).ToString());
                                        _typeConverters[objEntryType] = converter;
                                    }

                                    var ln = new LiteralNode(converter.ConvertToString(record[key]), objTypeUri);
                                    node = ValuedNodeExtensions.AsValuedNode(ln);
                                }
                                variableMap = variableMap.Append(new KeyValuePair<string, INode>(key, node));
                            }
                        }

                        SparqlResult sr = new SparqlResult(variableMap);
                        return new SparqlResultSet(new List<SparqlResult>() { sr });
                    }
                }
            }

            // If the query did not return a graph or result set, return an empty graph
            return new Graph();
        }

        private Triple VariableBoundResultToTriple(Neo4j.Driver.IRecord record, string subjectKey, string predicateKey, string objectKey, string relationType)
        {
            INode subject = new UriNode(new Uri(record[subjectKey].ToString()));
            INode predicate = new UriNode(new Uri(record[predicateKey].ToString()));
            INode obj = null;

            if (relationType.Equals("prop", StringComparison.OrdinalIgnoreCase))
            {
                var objEntry = record[objectKey];
                var objTypeUri = TypeConversionService.GetTypeUriForObject(objEntry);

                obj = new LiteralNode(TypeConversionService.ConvertToString(objEntry, objTypeUri.ToString(), false), objTypeUri);
            }
            else if (relationType.Equals("rel", StringComparison.OrdinalIgnoreCase))
            {
                if (Uri.TryCreate(record[objectKey].ToString(), UriKind.Absolute, out Uri uri))
                {
                    obj = new UriNode(uri);
                }
            }

            return new Triple(subject, predicate, obj);
        }

        private class TripleKeys
    {
        public bool CanCreateTriples => !string.IsNullOrEmpty(SubjectKey) &&
                                              !string.IsNullOrEmpty(PredicateKey) &&
                                              !string.IsNullOrEmpty(ObjectKey) &&
                                              !string.IsNullOrEmpty(RelationTypeKey);
        public string SubjectKey { get; set; }
        public string PredicateKey { get; set; }
        public string ObjectKey { get; set; }
        public string RelationTypeKey { get; set; }
        }

        private static string[] SubjectKeys = new string[] { "subject", "s", "subj" };
        private static string[] PredicateKeys = new string[] { "predicate", "p", "pred" };
        private static string[] ObjectKeys = new string[] { "object", "o", "obj" };
        private static string[] RelationTypeKeys = new string[] { "relationtype", "rt", "reltype" };
        private TripleKeys CanKeysBeUsedAsTriples(string[] keys)
        {
            TripleKeys tripleKeys = new TripleKeys();
            if (keys.Length == 4)
            {
                for (int i = 0; i < keys.Length; i++)
                {
                    if (SubjectKeys.Contains(keys[i].ToLowerInvariant()))
                    {
                        tripleKeys.SubjectKey = keys[i];
                    }
                    else if (PredicateKeys.Contains(keys[i].ToLowerInvariant()))
                    {
                        tripleKeys.PredicateKey = keys[i];
                    }
                    else if (ObjectKeys.Contains(keys[i].ToLowerInvariant()))
                    {
                        tripleKeys.ObjectKey = keys[i];
                    }
                    else if (RelationTypeKeys.Contains(keys[i].ToLowerInvariant()))
                    {
                        tripleKeys.RelationTypeKey = keys[i];
                    }
                }
            }

            return tripleKeys;
        }

        public override void ExecuteUpdate<T>(T query)
        {
            if (query is not CypherParameterizedString)
            {
                throw new ArgumentException("Query must be of type CypherParameterizedString", nameof(query));
            }

            CypherParameterizedString parameterizedString = query as CypherParameterizedString;
            Connection.Query(parameterizedString);
        }

        public override ICollectionOperationStrategy GetCollectionCreationStrategy(GraphUpdateContext context)
        {
            return new CollectionCreationStrategy(context);
        }

        public override ICollectionOperationStrategy GetCollectionUpdateStrategy(GraphUpdateContext context)
        {
            return new CollectionUpdaterStrategy(context);
        }

        protected override object ComposeFindSubjectQuery(Uri objectTypeUri, Uri typePredicateUri)
        {
            SparqlParameterizedString query = new SparqlParameterizedString("SELECT ?subject WHERE { ?subject @rdfType @objectTypeUri } LIMIT 100");
            query.SetUri("rdfType", typePredicateUri);
            query.SetUri("property", objectTypeUri);
            return query;
        }

        /// <summary>
        /// Asserts and retracts triples in both the remote and local working graphs.
        /// </summary>
        public override void AssertAndRetractRemote(TriplesSet triplesSet)
        {
            Connection.UpdateGraph(Connection.GraphUri, triplesSet.Assert, triplesSet.Retract);
        }

        public override int TriplesCount()
        {
            CypherParameterizedString query = new CypherParameterizedString("MATCH ()-[p]->() RETURN count(p) AS Triples");
            var result = ExecuteQuery(query);
            if (result is SparqlResultSet)
            {
                var rs = (SparqlResultSet)result;
                if (rs.Count > 0)
                {
                    var vn = rs[0]["Triples"].AsValuedNode();
                    return (int)vn.AsInteger();
                }
            }

            throw new Exception("Failed to retrieve triple count from remote graph.");
        }

        /// <summary>
        /// Executes a collection operation (e.g., SPARQL update) on both remote and local graphs.
        /// </summary>
        public override void ExecuteCollectionResult(CollectionOperationResult cor)
        {
            CypherParameterizedString query = new CypherParameterizedString(cor.Query);
            foreach (var param in cor.QueryParameters)
            {
                query.Parameters.Add(param.Key, param.Value);
            }

            ExecuteUpdate(query);
            LocalWorkingGraph.ExecuteUpdate(query);
        }


        public override GraphStatistics GetStatistics()
        {
            // Query for count of triples, subjects, predicates, objects, and literals
            string query = @"
                MATCH (s)-[p]->(o)
                RETURN 
                    count(s) AS Subjects, 
                    count(p) AS Predicates, 
                    count(o) AS Objects, 
                    count(CASE WHEN o IS NULL THEN 1 END) AS Literals, 
                    count(*) AS Triples";

            var result = ExecuteQuery(query);
            if (result is SparqlResultSet)
            {
                var rs = (SparqlResultSet)result;
                if (rs.Count > 0)
                {
                    var row = rs[0];
                    GraphStatistics gs = new GraphStatistics()
                    {
                        LiteralsCount = (int)row["Literals"].AsValuedNode().AsInteger(),
                        ObjectsCount = (int)row["Objects"].AsValuedNode().AsInteger(),
                        PredicatesCount = (int)row["Predicates"].AsValuedNode().AsInteger(),
                        SubjectsCount = (int)row["Subjects"].AsValuedNode().AsInteger(),
                        TriplesCount = (int)row["Triples"].AsValuedNode().AsInteger()
                    };

                    return gs;
                }
            }

            throw new Exception("Failed to retrieve graph statistics from remote graph.");
        }
    }
}
