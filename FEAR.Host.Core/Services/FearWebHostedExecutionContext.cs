using FEAR.Domain.Arguments;
using FEAR.Domain.KnowledgeGraph.GraphManager;
using FEAR.Domain.Telemetry;
using FEAR.Runtime.DataFormats;
using FEAR.Runtime.Execution;
using FEAR.Runtime.Helpers;
using FEAR.Runtime.KnowledgeGraph;
using Microsoft.Extensions.Logging;
using VDS.RDF.Parsing;
using VDS.RDF.Query;

namespace FEAR.Host.Core.Services
{
    public class FEARWebHostedTelemetry
    {

        public static void SendTelemetrySignal(IFEARTelemetrySignal signal, IFEARTelemetrySignalService telemetryService)
        {
            if (telemetryService != null)
            {
                telemetryService.SendSignal(signal);
            }
        }


        public static void SendExecutionTelemetry(string message, IFEARTelemetrySignalService telemetryService, FEARTelemetrySignalTypeEnum telemetrySignalType = FEARTelemetrySignalTypeEnum.Environment)
        {
            SendTelemetrySignal(new GenericFEARTelemetrySignal(FearWebHostedExecutionContext.FEARHOSTED_SOURCE, FEARTelemetrySerializationEnum.Raw, telemetrySignalType)
                .WithSignalData(message), telemetryService);
        }
    }

    /// <summary>
    /// Provides a hosted execution context for FEAR in a web environment.
    /// Handles artifact processing, graph management, background graph writing, and SPARQL querying.
    /// </summary>
    public class FearWebHostedExecutionContext : FearExecutionContext<FearHostedArguments>
    {
        public const string FEARHOSTED_SOURCE = "FearWebHostedExecutionContext";

        // Tracks the last time an artifact was posted
        private DateTime lastArtifactPost = DateTime.Now;
        // Tracks the last time the graph was written to disk
        private DateTime lastGraphWrite = DateTime.MinValue;

        private IFEARTelemetryDestination TelemetryDestination { get; set; }

        // Background thread for monitoring and writing the graph to disk
        Thread threadWriteWatcher;
        // Factory for resolving data format contexts based on content type
        private DataFormatContextFactory _dataFormatContextFactory = new DataFormatContextFactory();

        /// <summary>
        /// Initializes a new instance of the <see cref="FearWebHostedExecutionContext"/> class.
        /// Starts the background thread for graph writing.
        /// </summary>
        public FearWebHostedExecutionContext(
            IFEARTelemetrySignalService telemetryService,
            IFEARTelemetryDestinationProvider destinationProvider,
            IServiceProvider serviceProvider,
            IGraphManager graphManager,
            FearExecutionOptions<FearHostedArguments> executionArguments,
            Runtime.Environment fearEnvironment)
            : base(telemetryService, destinationProvider, serviceProvider, graphManager, executionArguments, fearEnvironment)
        {
            TelemetryDestination = TelemetryProvider.RegisterDestination("LogFile", FearExecutionOptions.DefaultTelemetryServiceSource, new Dictionary<string, object> { { "LogFileName", $"fear-default-{DateTime.UtcNow:yyyyMMddHHmm}.log" } });
            threadWriteWatcher = new Thread(WriteWatcher);
            threadWriteWatcher.Start();
        }

        public void Execute(string data, string contentType)
        {
            IDataFormatContext dataFormatContext = _dataFormatContextFactory.Create(contentType);
            IDataFormatContextState state = dataFormatContext.CreateStateFromContent(data);

            var execCtx = new NonReturningArtifactPipelineExecution(state);
            base.Execute(execCtx);
        }

        public IDataFormatContextState CreateState(string contentType, string data)
        {
            IDataFormatContext dataFormatContext = _dataFormatContextFactory.Create(contentType);
            IDataFormatContextState state = dataFormatContext.CreateStateFromContent(data);
            return state;
        }

        /// <summary>
        /// Processes incoming artifact data in the specified content type.
        /// Converts the data to a format context state and executes it in the base context.
        /// </summary>
        /// <param name="data">The artifact data as a string.</param>
        /// <param name="contentType">The MIME type of the data.</param>
        public void ProcessArtifacts(IArtifactPipelineExecution ctx)
        {
            base.Execute(ctx);
        }

        /// <summary>
        /// Updates the last artifact post time after processing items in the queue.
        /// </summary>
        /// <param name="count">The number of items processed.</param>
        public override void PostRunQueueAction(int count)
        {
            if (count > 0)
            {
                lastArtifactPost = DateTime.Now;
            }
        }

        /// <summary>
        /// Background thread method that periodically writes the materialized graph to disk
        /// if new artifacts have been posted and a certain time has elapsed.
        /// Also outputs graph statistics to the console in JSON format.
        /// </summary>
        private void WriteWatcher()
        {
            while (true)
            {
                try
                {
                    if (lastGraphWrite < lastArtifactPost)
                    {
                        var now = DateTime.Now;
                        // Write the graph if 20 seconds have passed since the last artifact post
                        if (now - lastArtifactPost > TimeSpan.FromSeconds(20))
                        {
                            using (FileStream fs = File.OpenWrite(Path.Combine(ExecutionOptions.TypedArguments.GetWorkingDirectory(), "graph.ttl")))
                            {
                                fs.SetLength(0);
                                using (Stream workingStream = FileStream.Synchronized(fs))
                                {
                                    // Graph writing logic would go here (currently commented out)
                                    // GraphHelpers.WriteGraph("", GraphManager.MaterializedGraph.Graph, new List<Stream>() { Console.OpenStandardOutput(), workingStream });
                                    lastGraphWrite = now;

                                    // Output graph statistics
                                    var totalTriples = GraphManager.MaterializedGraph.Graph.Triples.Count();
                                    var totalSubjects = GraphManager.MaterializedGraph.Graph.Triples.Select(t => t.Subject).Distinct().Count();
                                    var totalPredicates = GraphManager.MaterializedGraph.Graph.Triples.Select(t => t.Predicate).Distinct().Count();
                                    var totalObjects = GraphManager.MaterializedGraph.Graph.Triples.Select(t => t.Object).Distinct().Count();

                                    FEARWebHostedTelemetry.SendExecutionTelemetry($"Graph Written - Triples: {totalTriples}, Subjects: {totalSubjects}, Predicates: {totalPredicates}, Objects: {totalObjects}", null);
                                }
                            }
                        }
                    }
                } catch { }

                Thread.Sleep(2000); // Sleep for 2 seconds before checking again
            }
        }

        /// <summary>
        /// Executes a SPARQL query against the materialized graph.
        /// Supports both remote and local graph implementations.
        /// </summary>
        /// <param name="query">The SPARQL query string.</param>
        /// <returns>The result of the query execution.</returns>
        public Object Query(string query)
        {
            if (GraphManager.MaterializedGraph is BaseRemoteGraph)
            {
                var remoteGraph = GraphManager.MaterializedGraph as BaseRemoteGraph;
                //SparqlParameterizedString sps = new SparqlParameterizedString(query);
                return remoteGraph.ExecuteQuery(query);
            }
            else
            {
                var localGraph = GraphManager.MaterializedGraph as LocalGraph;
                SparqlQueryParser sparqlQueryParser = new SparqlQueryParser();
                SparqlQuery sparqlQuery = sparqlQueryParser.ParseFromString(query);
                return localGraph.ExecuteQuery(sparqlQuery);
            }
        }
    }
}
