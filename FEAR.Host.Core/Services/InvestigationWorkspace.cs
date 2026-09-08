using FEAR.Domain.Agents;
using FEAR.Domain.KnowledgeGraph.Graphs;
using FEAR.Domain.Model;
using FEAR.Domain.Telemetry;
using FEAR.Host.Domain.Api.Investigation;
using FEAR.Runtime.DataFormats;
using FEAR.Runtime.Domain;
using FEAR.Runtime.Execution;
using Jitbit.Utils;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Security.Claims;
using System.Security.Cryptography;
using VDS.RDF;
using VDS.RDF.Parsing;
using VDS.RDF.Writing;
using FEAR.Host.Core.Agents;
using FEAR.Runtime.Agents;

namespace FEAR.Host.Core.Services
{
    public class PostArtifactContext
    {
        public string JsonData { get; set; }
        public string ContentType { get; set; }
        public HttpContext HttpContext { get; set; }

        public IArtifactPipelineExecution CreateArtifactPipelineExecution(FearWebHostedExecutionContext hostCtx, Action<IEphemeralGraph, IArtifactPipelineExecution> onReturn)
        {
            IDataFormatContextState state = hostCtx.CreateState(ContentType, JsonData);
            return new ReturningArtifactPipelineExecutionContext(state, onReturn);
        }
    }

    /// <summary>
    /// Service for managing the lifecycle and secure access to an Autopsy case graph.
    /// Handles initialization, artifact posting, querying, and user token management.
    /// </summary>
    public class InvestigationWorkspace
    {
        /// <summary>
        /// The name of the case this service manages.
        /// </summary>
        public string InvestigationName { get; set; }

        /// <summary>
        /// The configuration used for initializing the case context.
        /// </summary>
        public CaseConfiguration Configuration { get; set; }

        private IHost container;

        /// <summary>
        /// The execution context for the hosted FEAR web environment.
        /// </summary>
        private FearWebHostedExecutionContext context = null;

        protected Lazy<AgentAdapterContextProvider> _agentAdatperContextProvider { get; set; }
        protected Lazy<IAgentAdapterRegistry> _AgentAdapterProvider { get; set; }
        protected Lazy<IFEARTelemetrySignalService> _telemetryService { get; set; }
        protected Lazy<IAgentAdapterQueueProvider> _agentRequestQueueProvider { get; set; }
        protected DataSigningService _dataSigningService { get; set; }
        protected CancellationTokenSource _contextCancellationTokenSource { get; set; }
        public IAgentAdapterRegistry AgentAdapterProvider => _AgentAdapterProvider.Value;
        public IFEARTelemetrySignalService TelemetryService => _telemetryService.Value;

        protected IConfiguration ApplicationConfiguration { get; }
        public string InvestigationOntology => context.OntologyStore.GetOntology();

        /// <summary>
        /// Constructs a new AutopsyCaseGraphService for a given case and configuration.
        /// Generates a random secret for token generation.
        /// </summary>
        /// <param name="invName">The name of the case.</param>
        /// <param name="configuration">The configuration for the case.</param>
        /// <param name="applicationConfiguation">The application configuration.</param>
        public InvestigationWorkspace(CaseConfiguration configuration, IConfiguration applicationConfiguation, DataSigningService dataSigningService)
        {
            Configuration = configuration;
            _dataSigningService = dataSigningService;
            InvestigationName = configuration.InvestigationName;
            ApplicationConfiguration = applicationConfiguation;
            _contextCancellationTokenSource = new CancellationTokenSource();

        }

        /// <summary>
        /// Initializes the FEAR execution context for this case using the provided configuration.
        /// Sets up dependency injection and starts required services.
        /// </summary>
        public void Initialize()
        {
            // Create execution options and compiler options for the hosted context
            FearExecutionOptions<FearHostedArguments> fearHostedOptions = new FearHostedOptions(Configuration.ToConfiguration(), ApplicationConfiguration, FearWebHostedExecutionContext.FEARHOSTED_SOURCE);
            FEARCompilerOptions compilerOptions = FEARCompilerOptions.DefaultCompilerOptions(fearHostedOptions, (result) => { });

            // Build the host with all required services
            HostApplicationBuilder builder = FearExecutionContext<FearHostedArguments>.DefaultBuilder<FearWebHostedExecutionContext>(fearHostedOptions, compilerOptions);

            builder.Services.AddSingleton(Configuration);
            builder.Services.AddSingleton(this);

            builder.AddAgentAdapters(_dataSigningService);
            
            container = builder.Build();
            var services = container.Services.GetRequiredService<IEnumerable<IHostedService>>();
            container.RunAsync(_contextCancellationTokenSource.Token);

            // Retrieve the execution context from the service container
            context = container.Services.GetService<FearWebHostedExecutionContext>();

            _AgentAdapterProvider = new Lazy<IAgentAdapterRegistry>(() => container.Services.GetService<IAgentAdapterRegistry>());
            _telemetryService = new Lazy<IFEARTelemetrySignalService>(() => container.Services.GetService<IFEARTelemetrySignalService>());
            _agentRequestQueueProvider = new Lazy<IAgentAdapterQueueProvider>(() => container.Services.GetService<IAgentAdapterQueueProvider>());
            _agentAdatperContextProvider = new Lazy<AgentAdapterContextProvider>(() => container.Services.GetService<AgentAdapterContextProvider>());

            // Start background and required services
            context.StartServices();
        }

        /// <summary>
        /// Shuts down the FEAR execution context for this case, stopping all services.
        /// </summary>
        public void Shutdown()
        {
            context?.StopServices();

            _contextCancellationTokenSource.Cancel();

            container.StopAsync().Wait();
            container.Dispose();
        }

        /// <summary>
        /// Posts artifact data to the case graph, after verifying the user's token.
        /// Throws UnauthorizedAccessException if the token is invalid.
        /// </summary>
        /// <param name="jsonData">The artifact data in JSON format.</param>
        /// <param name="contentType">The content type of the data.</param>
        /// <param name="uct">The user case token for authentication.</param>
        public Guid PostArtifacts(PostArtifactContext ctx)
        {
            var pipelineExc = ctx.CreateArtifactPipelineExecution(context, onReturn);

            TelemetryService.SendSignal(new GenericFEARTelemetrySignal(FearWebHostedExecutionContext.FEARHOSTED_SOURCE, FEARTelemetrySerializationEnum.Raw, FEARTelemetrySignalTypeEnum.Trace).WithSignalData(
                $"Posting artifacts for investigation: {InvestigationName}\nReceived Count:{pipelineExc.State.Data.Count()}\nUser name: {ctx.HttpContext.User.Identity.Name}\nSource: {ctx.HttpContext.Connection.RemoteIpAddress}"
                ));

            context.ProcessArtifacts(pipelineExc);
            return pipelineExc.ExecutionInstanceId;
        }

        FastCache<Guid, string> workItemResultCache = new FastCache<Guid, string>(1000);
        private void onReturn(IEphemeralGraph data, IArtifactPipelineExecution ctx)
        {
            // No action needed on return for this implementation
            if (!workItemResultCache.TryGet(ctx.ExecutionInstanceId, out var graph))
            {
                lock (workItemResultCache)
                {
                    MemoryStream ms = new MemoryStream();
                    TextWriter writer = new StreamWriter(ms, leaveOpen: true);
                    string contentType = "";


                    VDS.RDF.Graph g = data.Graph;
                    CompressingTurtleWriter tw = new CompressingTurtleWriter(5, TurtleSyntax.Rdf11Star);
                    tw.DefaultNamespaces.Import(g.NamespaceMap);
                    tw.Save(g, writer);

                    StreamReader sr = new StreamReader(ms);
                    ms.Position = 0;
                    string result = sr.ReadToEnd();

                    workItemResultCache.AddOrUpdate(ctx.ExecutionInstanceId, result, TimeSpan.FromSeconds(30));
                }
            }
        }

        public object GetArtifactWorkItemResult(Guid id)
        {
            if (workItemResultCache.TryGet(id, out var res))
                return res;
            return null;
        }

        /// <summary>
        /// Executes a query against the case graph and returns the result.
        /// </summary>
        /// <param name="query">The query string to execute.</param>
        /// <returns>The result of the query.</returns>
        public object Query(string query)
        {
            return context.Query(query);
        }

        /// <summary>
        /// Retrieves telemetry signals from all registered telemetry destinations.
        /// </summary>
        /// <param name="since">The starting point in time from which to retrieve signals.</param>
        /// <param name="count">The maximum number of signals to retrieve.</param>
        /// <returns>A list of telemetry signals.</returns>
        public List<IFEARTelemetrySignal> GetTelemetrySignals(DateTime? since = null, int? count = null)
        {
            var telemetryService = context.ServiceProvider.GetService<IFEARTelemetrySignalService>();
            if (telemetryService == null)
                return new List<IFEARTelemetrySignal>();

            var destinationProvider = context.ServiceProvider.GetService<IFEARTelemetryDestinationProvider>();
            if (destinationProvider == null)
                return new List<IFEARTelemetrySignal>();

            var destinations = destinationProvider.GetDestinations();
            var all = new List<IFEARTelemetrySignal>();

            foreach (var dest in destinations.Values)
            {
                if (since.HasValue) all.AddRange(dest.GetSignalsSince(since.Value));
                else if (count.HasValue) all.AddRange(dest.GetLastNSignals(count.Value));
                else all.AddRange(dest.GetSignals());
            }

            return all.OrderByDescending(s => s.Timestamp).ToList();
        }

        public void EnqueueAgentRequest(QueuedAgentRequest queuedAgentRequest)
        {
            _agentRequestQueueProvider.Value.Enqueue(queuedAgentRequest);
        }

        public AgentExecutionContext CreateContext(string username, string invName)
        {
            return _agentAdatperContextProvider.Value.CreateContext(username, invName);
        }

        public AgentExecutionContext AddEntryToContext(string ctxId, string username, AgentInteraction ctxEntry)
        {
            return _agentAdatperContextProvider.Value.AddEntryToContext(ctxId, username, ctxEntry);
        }

        public bool TryGetContext(string ctxId, string username, out AgentExecutionContext? responseCtx)
        {
            return _agentAdatperContextProvider.Value.TryGetContext(ctxId, username, out responseCtx);
        }

        /// <summary>
        /// Returns statistics about the investigation's materialized knowledge graph.
        /// </summary>
        public GetInvestigationGraphStatistics.Response GetGraphStatistics()
        {
            if (context?.GraphManager?.MaterializedGraph == null)
                return new GetInvestigationGraphStatistics.Response { IsAvailable = false };

            var graphStatistics = context.GraphManager.MaterializedGraph.GetStatistics();
            
            return new GetInvestigationGraphStatistics.Response
            {
                IsAvailable = true,
                TriplesCount = graphStatistics.TriplesCount,
                SubjectsCount = graphStatistics.SubjectsCount,
                PredicatesCount = graphStatistics.PredicatesCount,
                ObjectsCount = graphStatistics.ObjectsCount,
                LiteralsCount = graphStatistics.LiteralsCount
            };
        }

        /// <summary>
        /// Serializes the investigation's materialized knowledge graph to Turtle (TTL) format.
        /// </summary>
        public string GetGraphAsTurtle()
        {
            if (context?.GraphManager?.MaterializedGraph == null)
                return null;

            var rdfGraph = context.GraphManager.MaterializedGraph.Graph;
            var writer = new CompressingTurtleWriter(5, TurtleSyntax.Rdf11Star);
            writer.DefaultNamespaces.Import(rdfGraph.NamespaceMap);
            using var sw = new System.IO.StringWriter();
            writer.Save(rdfGraph, sw);
            return sw.ToString();
        }
    }
}
