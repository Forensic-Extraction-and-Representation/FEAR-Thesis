using FEAR.Domain.KnowledgeGraph;
using FEAR.Domain.KnowledgeGraph.Collections;
using FEAR.Domain.KnowledgeGraph.EntitySearch;
using FEAR.Domain.KnowledgeGraph.GraphCodify;
using FEAR.Domain.KnowledgeGraph.GraphDB.Fuseki;
using FEAR.Domain.KnowledgeGraph.Graphs;
using FEAR.Domain.KnowledgeGraph.GraphUpdate;
using FEAR.Domain.KnowledgeGraph.TypeService;
using FEAR.Domain.Telemetry;
using FEAR.Runtime.KnowledgeGraph.Sparql.Collections;
using System.Text;
using VDS.RDF;
using VDS.RDF.Nodes;
using VDS.RDF.Parsing;
using VDS.RDF.Query;

namespace FEAR.Runtime.KnowledgeGraph.Sparql
{
    /// <summary>
    /// Represents a remote (e.g., Fuseki-backed) materialized knowledge graph in FEAR.
    /// Provides logic for synchronizing between a remote graph and a local working graph.
    /// </summary>
    public class FusekiRemoteGraph : BaseRemoteGraph, IRemoteGraph<SparqlParameterizedString>
    {
        public override string GraphLanguage => "SPARQL";
        private FusekiGraphDBConnectionFactory ConnectionFactory { get; set; }
        private FusekiGraphDBConnection Connection { get; set; }

        SparqlQueryParser queryParser = new SparqlQueryParser();
        SparqlUpdateParser updateParser = new SparqlUpdateParser();
        ISparqlQueryProcessor sparqlQueryProcessor = null;
        private IFEARTelemetrySignalService TelemetryService { get; set; }
        public override string GraphName => Connection.GraphUri;

        /// <summary>
        /// Initializes a new instance of the <see cref="RemoteGraph"/> class.
        /// Sets up connection, strategies, and local working graph.
        /// </summary>
        public FusekiRemoteGraph(FusekiGraphDBConnectionFactory connectionFactory, FindEntityStrategyFactoryManager findEntityStrategyFactoryManager,
            ITypeConversionService typeConversionService, IGraphCodifyServiceConfiguration configuration,
            IFEARTelemetrySignalService telemetryService = null)
            : base(typeConversionService, configuration, findEntityStrategyFactoryManager)
        {
            ConnectionFactory = connectionFactory;
            Connection = ConnectionFactory.ConstructConnection();
            sparqlQueryProcessor = Connection.CreateSparqlQueryProcessor();
            TelemetryService = telemetryService;
        }

        /// <summary>
        /// Executes a collection operation (e.g., SPARQL update) on both remote and local graphs.
        /// </summary>
        public override void ExecuteCollectionResult(CollectionOperationResult cor)
        {
            SparqlParameterizedString query = new SparqlParameterizedString(cor.Query);
            foreach (var param in cor.QueryParameters)
            {
                query.SetParameter(param.Key, param.Value as INode);
            }

            ExecuteUpdate(query);
            LocalWorkingGraph.ExecuteUpdate(query);
        }

        /// <summary>
        /// Executes a SPARQL update query on the remote graph.
        /// </summary>
        public override void ExecuteUpdate<T>(T query)
        {
            if (query is not SparqlParameterizedString && query is not string)
            {
                throw new ArgumentException("Query must be a SparqlParameterizedString", nameof(query));
            }

            SparqlParameterizedString sparqlQuery = null;

            if (query is string)
            {
                sparqlQuery = new SparqlParameterizedString(query as string);
            }
            else if (query is SparqlParameterizedString)
            {
                sparqlQuery = query as SparqlParameterizedString;
            }

            try
            {
                var spcs = updateParser.ParseFromString(sparqlQuery);
                Connection.Update(spcs.ToString());
            }
            catch (Exception ex)
            {
                var diagnostic = BuildDiagnosticMessage("SPARQL UPDATE", sparqlQuery, ex);
                SendErrorTelemetry(diagnostic);
                throw new Exception(diagnostic, ex);
            }
        }

        /// <summary>
        /// Executes a SPARQL query on the remote graph and returns the result.
        /// </summary>
        public override object ExecuteQuery<T>(T query)
        {
            if (query is not SparqlParameterizedString && query is not string)
            {
                throw new ArgumentException("Query must be a SparqlParameterizedString", nameof(query));
            }

            SparqlParameterizedString sparqlQuery = null;

            if (query is string)
            {
                sparqlQuery = new SparqlParameterizedString(query as string);
            }
            else if (query is SparqlParameterizedString)
            {
                sparqlQuery = query as SparqlParameterizedString;
            }

            try
            {
                // Parse the query and set the default graph if needed
                var spq = queryParser.ParseFromString(sparqlQuery);
                if (!string.IsNullOrEmpty(Connection.GraphUri))
                    spq.AddDefaultGraph(new Uri(Connection.GraphUri));

                object returnObj = sparqlQueryProcessor.ProcessQuery(spq);
                // If the result is a graph, import namespaces for consistency
                if (returnObj is Graph)
                {
                    (returnObj as Graph).NamespaceMap.Import(sparqlQuery.Namespaces);
                }

                return returnObj;
            }
            catch (Exception ex)
            {
                var diagnostic = BuildDiagnosticMessage("SPARQL SELECT/CONSTRUCT/ASK", sparqlQuery, ex);
                SendErrorTelemetry(diagnostic);
                throw new Exception(diagnostic, ex);
            }
        }

        /// <summary>
        /// Executes a SPARQL CONSTRUCT query and asserts the results into the local working graph.
        /// </summary>
        public void ExecuteConstructQuery(SparqlParameterizedString query)
        {
            ExecuteConstructQuery(query, LocalWorkingGraph);
        }

        /// <summary>
        /// Executes a SPARQL CONSTRUCT query and asserts the results into the specified local graph.
        /// </summary>
        public void ExecuteConstructQuery(SparqlParameterizedString query, IRealGraph localGraph)
        {
            var result = ExecuteQuery(query);
            if (result is Graph)
            {
                var g = (Graph)result;

                foreach (var triple in g.Triples)
                {
                    // Assert each triple into the local graph
                    localGraph.Graph.Assert(triple);
                }
            }
        }

        /// <summary>
        /// Builds a detailed diagnostic message for a failed SPARQL operation, including
        /// connection details, the full query text, and the complete exception chain with stack traces.
        /// </summary>
        private string BuildDiagnosticMessage(string operationType, SparqlParameterizedString query, Exception ex)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"[FusekiRemoteGraph] {operationType} execution failed.");
            sb.AppendLine($"  Graph URI   : {Connection?.GraphUri ?? "(unknown)"}");
            sb.AppendLine($"  Graph Name  : {GraphName}");
            sb.AppendLine();
            sb.AppendLine("--- Query ---");
            sb.AppendLine(query?.ToString() ?? "(null)");
            sb.AppendLine();
            sb.AppendLine("--- Exception Chain ---");
            var current = ex;
            int depth = 0;
            while (current != null)
            {
                sb.AppendLine($"[{depth}] {current.GetType().FullName}: {current.Message}");
                if (!string.IsNullOrWhiteSpace(current.StackTrace))
                {
                    sb.AppendLine(current.StackTrace);
                }
                current = current.InnerException;
                depth++;
                if (current != null)
                    sb.AppendLine("--- Inner Exception ---");
            }
            return sb.ToString();
        }

        /// <summary>
        /// Sends an error-level telemetry signal with the provided diagnostic message, if a telemetry service is configured.
        /// </summary>
        private void SendErrorTelemetry(string diagnosticMessage)
        {
            TelemetryService?.SendSignal(
                new GenericFEARTelemetrySignal(nameof(FusekiRemoteGraph), FEARTelemetrySerializationEnum.Raw, FEARTelemetrySignalTypeEnum.Error)
                    .WithSignalData(diagnosticMessage));
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

        /// <summary>
        /// Clears all triples from both the remote and local working graphs.
        /// </summary>
        public override void Clear()
        {
            // Delete all triples from the remote graph
            SparqlParameterizedString query = new SparqlParameterizedString("DELETE { ?s ?p ?o } WHERE { ?s ?p ?o}");
            ExecuteUpdate(query);
            // Re-initialize the local working graph
            _localWorkingGraph = new Lazy<LocalGraph>(ConstructLocalWorkingGraph);
            LocalWorkingGraph.Clear();
        }

        /// <summary>
        /// Gets the total number of triples in the remote graph.
        /// </summary>
        public override int TriplesCount()
        {
            SparqlParameterizedString query = new SparqlParameterizedString("SELECT (COUNT(*) as ?Triples) WHERE { ?s ?p ?o}");
            var result = ExecuteQuery(query);
            if (result is SparqlResultSet)
            {
                var rs = (SparqlResultSet)result;
                if (rs.Count > 0)
                {
                    return (int)rs[0]["Triples"].AsValuedNode().AsInteger();
                }
            }

            throw new Exception("Failed to retrieve triple count from remote graph.");
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
            // Query for count of triples, subjects, predicates, objects, and literals
            string query = @"
                PREFIX rdf: <http://www.w3.org/1999/02/22-rdf-syntax-ns#>
                SELECT (COUNT(*) AS ?Triples) 
                       (COUNT(DISTINCT ?s) AS ?Subjects) 
                       (COUNT(DISTINCT ?p) AS ?Predicates) 
                       (COUNT(DISTINCT ?o) AS ?Objects) 
                       (COUNT(DISTINCT ?l) AS ?Literals)
                WHERE {
                    ?s ?p ?o .
                    OPTIONAL { ?o a rdf:Literal . BIND(?o AS ?l) }
                }";

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
