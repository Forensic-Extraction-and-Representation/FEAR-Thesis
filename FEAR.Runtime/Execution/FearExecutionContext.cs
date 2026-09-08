using FEAR.CFEAR;
using FEAR.Domain.Arguments;
using FEAR.Domain.Collector;
using FEAR.Domain.GraphCodifier;
using FEAR.Domain.Infrastructure;
using FEAR.Domain.KnowledgeGraph.EntitySearch;
using FEAR.Domain.KnowledgeGraph.GraphCodify;
using FEAR.Domain.KnowledgeGraph.GraphManager;
using FEAR.Domain.KnowledgeGraph.Graphs;
using FEAR.Domain.KnowledgeGraph.IRI;
using FEAR.Domain.KnowledgeGraph.TypeService;
using FEAR.Domain.Model;
using FEAR.Domain.Ontology;
using FEAR.Domain.Provenance;
using FEAR.Domain.RuleSet;
using FEAR.Domain.Telemetry;
using FEAR.GFEAR;
using FEAR.Runtime.CFEAR.CollectorResults;
using FEAR.Runtime.Compiler;
using FEAR.Runtime.Domain;
using FEAR.Runtime.KnowledgeGraph;
using FEAR.Runtime.KnowledgeGraph.EntitySearch.InMemory;
using FEAR.Runtime.Ontology;
using FEAR.Runtime.Telemetry;
using FEAR.Runtime.ToolResolver;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Security.Cryptography;
using VDS.RDF.Query;

namespace FEAR.Runtime.Execution
{
    /// <summary>
    /// Provides the execution context for FEAR artifact processing, including queue management,
    /// collector and codifier resolution, and execution orchestration.
    /// </summary>
    /// <typeparam name="T">The type of arguments used for execution.</typeparam>
    public abstract class FearExecutionContext<T> : BaseQueueService, IArtifactWorkItemQueueService
        where T : FearArguments
    {
        public const string FEARHOSTED_SOURCE = "FearExecutionContext";

        /// <summary>
        /// Queue for raw data items to be processed by collectors.
        /// </summary>
        public ConcurrentQueue<ArtifactWorkItem> WorkItemQueue { get; set; } = new ConcurrentQueue<ArtifactWorkItem>();

        /// <summary>
        /// The execution options for this context.
        /// </summary>
        protected FearExecutionOptions<T> ExecutionOptions { get; set; }

        /// <summary>
        /// Hash algorithm used for property-based codifier matching.
        /// </summary>
        private SHA1 hashAlgorithm = SHA1.Create();

        /// <summary>
        /// Logger for diagnostic and error output.
        /// </summary>
        protected IFEARTelemetrySignalService TelemetryService;

        /// <summary>
        /// Lazily-initialized dictionary of graph codifiers, grouped by codifier name.
        /// </summary>
        private Lazy<Dictionary<string, List<IFEARGraphCodifier>>> _graphCodifiers;

        /// <summary>
        /// Lazily-initialized collection of result collectors.
        /// </summary>
        private Lazy<IEnumerable<ResultCollectorBase>> _collectors;

        /// <summary>
        /// Gets the dictionary of graph codifiers, grouped by codifier name.
        /// </summary>
        private Dictionary<string, List<IFEARGraphCodifier>> GraphCodifiers => _graphCodifiers.Value;

        /// <summary>
        /// Gets the collection of result collectors.
        /// </summary>
        private IEnumerable<ResultCollectorBase> Collectors => _collectors.Value;

        /// <summary>
        /// Lazily-initialized list of FEAR rule sets.
        /// </summary>
        private Lazy<List<IFEARRuleSet>> _rulesets;

        /// <summary>
        /// Used when dequeuing rule sets to keep track of the current index.
        /// Ensures that we can cycle through the available rule sets in a round-robin fashion.
        /// </summary>
        private int curentRuleSetIndex = 0;

        /// <summary>
        /// Gets the list of FEAR rule sets available in the environment.
        /// </summary>
        private List<IFEARRuleSet> Rulesets => _rulesets.Value;

        /// <summary>
        /// The FEAR runtime environment.
        /// </summary>
        private FEAR.Runtime.Environment _fearEnvironment;

        /// <summary>
        /// Lazily-initialized graph manager.
        /// </summary>
        private Lazy<IGraphManager> _graphManager = null;

        private Lazy<IServiceProvider> _serviceProvider = null;

        /// <summary>
        /// Gets the graph manager for this execution context.
        /// </summary>
        public IGraphManager GraphManager => _graphManager.Value;
        public IServiceProvider ServiceProvider => _serviceProvider.Value;

        public IFEAROntologyStore OntologyStore => _serviceProvider.Value.GetService<IFEAROntologyStore>();

        /// <summary>
        /// Initializes a new instance of the <see cref="FearExecutionContext{T}"/> class.
        /// </summary>
        /// <param name="logger">Logger for diagnostic output.</param>
        /// <param name="graphManager">The graph manager instance.</param>
        /// <param name="executionOptions">Execution options for this context.</param>
        /// <param name="fearEnvironment">The FEAR runtime environment.</param>
        public FearExecutionContext(IFEARTelemetrySignalService telemetryService, IFEARTelemetryDestinationProvider destinationProvider, IServiceProvider serviceProvider, IGraphManager graphManager, FearExecutionOptions<T> executionOptions, Runtime.Environment fearEnvironment) :
            base(telemetryService, destinationProvider, serviceProvider, executionOptions)
        {
            TelemetryService = telemetryService;
            ExecutionOptions = executionOptions;
            _fearEnvironment = fearEnvironment;

            _graphManager = new Lazy<IGraphManager>(() => graphManager);
            _serviceProvider = new Lazy<IServiceProvider>(() => serviceProvider);

            // Initialize graph codifiers and import their namespaces
            _graphCodifiers = new Lazy<Dictionary<string, List<IFEARGraphCodifier>>>(() =>
            {
                var gcl = _fearEnvironment.Compiler.Resolver.GraphCodifiers.ToList();

                TelemetryService.SendSignal(new GenericFEARTelemetrySignal(FEARHOSTED_SOURCE, FEARTelemetrySerializationEnum.Raw, FEARTelemetrySignalTypeEnum.Metric)
                    .WithSignalData($"Found {gcl.Count} graph codifiers in the environment."));
                TelemetryService.SendSignal(new GenericFEARTelemetrySignal(FEARHOSTED_SOURCE, FEARTelemetrySerializationEnum.Raw, FEARTelemetrySignalTypeEnum.Trace)
                    .WithSignalData(string.Join("\n", gcl.Select(gc => $"[-] {gc.CodifierName}"))));

                // Register ontology and namespace prefixes for each codifier.
                gcl.ForEach(t =>
                {
                    foreach (var kvp in t.OntologyNamespaceImports)
                        GraphManager.AddNamespace(kvp.Key, kvp.Value);

                    foreach (var kvp in t.NamespacePrefixes)
                        GraphManager.AddNamespace(kvp.Key, kvp.Value);
                });

                // Group codifiers by their name for efficient lookup.
                var gcDict = gcl.GroupBy(x => x.CodifierName).ToDictionary(x => x.Key, x => x.ToList());

                return gcDict;
            });

            // Initialize the rulesets from the FEAR environment.
            _rulesets = new Lazy<List<IFEARRuleSet>>(() =>
            {
                var rl = _fearEnvironment.Compiler.Resolver.RuleSets.ToList();

                TelemetryService.SendSignal(new GenericFEARTelemetrySignal(FEARHOSTED_SOURCE, FEARTelemetrySerializationEnum.Raw, FEARTelemetrySignalTypeEnum.Metric)
                    .WithSignalData($"Found {rl.Count} rule sets in the environment."));
                TelemetryService.SendSignal(new GenericFEARTelemetrySignal(FEARHOSTED_SOURCE, FEARTelemetrySerializationEnum.Raw, FEARTelemetrySignalTypeEnum.Trace)
                    .WithSignalData(string.Join("\n", rl.Select(rs => $"[-] {rs.RulesetName}"))));

                // Register ontology and namespace prefixes for each ruleset.
                rl.ForEach(t =>
                {
                    foreach (var kvp in t.OntologyNamespaceImports)
                        GraphManager.AddNamespace(kvp.Key, kvp.Value);

                    foreach (var kvp in t.NamespacePrefixes)
                        GraphManager.AddNamespace(kvp.Key, kvp.Value);
                });

                return rl;
            });

            // Initialize collectors for result objects
            _collectors = new Lazy<IEnumerable<ResultCollectorBase>>(() =>
            {
                var cs = _fearEnvironment.Compiler.Resolver.Collectors.ToList();

                TelemetryService.SendSignal(new GenericFEARTelemetrySignal(FEARHOSTED_SOURCE, FEARTelemetrySerializationEnum.Raw, FEARTelemetrySignalTypeEnum.Metric)
                    .WithSignalData($"Found {cs.Count} collectors in the environment."));
                TelemetryService.SendSignal(new GenericFEARTelemetrySignal(FEARHOSTED_SOURCE, FEARTelemetrySerializationEnum.Raw, FEARTelemetrySignalTypeEnum.Trace)
                    .WithSignalData(string.Join("\n", cs.Select(c => $"[-] {c.CollectorName}"))));

                var resultCollectors = cs.Where(t => t.CollectorType == CollectorTypeEnum.ResultObject).Cast<ResultCollectorBase>();

                TelemetryService.SendSignal(new GenericFEARTelemetrySignal(FEARHOSTED_SOURCE, FEARTelemetrySerializationEnum.Raw, FEARTelemetrySignalTypeEnum.Metric)
                    .WithSignalData($"Found {resultCollectors.Count()} result object collectors in the environment."));
                TelemetryService.SendSignal(new GenericFEARTelemetrySignal(FEARHOSTED_SOURCE, FEARTelemetrySerializationEnum.Raw, FEARTelemetrySignalTypeEnum.Trace)
                    .WithSignalData(string.Join("\n", resultCollectors.Select(rc => $"[-] {rc.CollectorName}"))));

                return resultCollectors;
            });
        }

        private Dictionary<string, Func<ArtifactWorkItem, ArtifactWorkResult>> _Stages => new Dictionary<string, Func<ArtifactWorkItem, ArtifactWorkResult>>() {
            { "Collectors", RunCollectorWorkItem },{ "Codifiers", RunCodifierWorkItem } };

        public List<KeyValuePair<string, bool>> CreatePipelineStages()
        {
            List<KeyValuePair<string, bool>> stages = new List<KeyValuePair<string, bool>>();
            if (Collectors.Any())
                stages.Add(new KeyValuePair<string, bool>("Collectors", false));
            if (GraphCodifiers.Any())
                stages.Add(new KeyValuePair<string, bool>("Codifiers", false));
            return stages;
        }

        /// <summary>
        /// Enqueues a collector result as raw data to be processed by collectors.
        /// </summary>
        /// <param name="result">The collector result to enqueue.</param>
        public void QueueArtifactWorkItem(ArtifactWorkItem result)
        {
            result.PipelineStages = CreatePipelineStages();
            WorkItemQueue.Enqueue(result);
        }

        /// <summary>
        /// Not implemented: Enqueues provenance information for processing.
        /// </summary>
        /// <param name="provenance">The provenance object.</param>
        public void QueueProvenance(IProvenance provenance)
        {
            throw new System.NotImplementedException();
        }

        /// <summary>
        /// Executes the context by processing the provided data state, enqueuing results for collectors and codifiers.
        /// </summary>
        /// <param name="state">The data format context state to process.</param>
        public virtual void Execute(IArtifactPipelineExecution ctx)
        {
            if (!QueueRunning)
                StartServices();

            IEnumerable<dynamic> dataList = ctx.State.Data;
            List<ArtifactWorkItem> collectorResults = new List<ArtifactWorkItem>();

            foreach (var element in dataList)
            {
                try
                {
                    ArtifactWorkItem cr = CreateArtifactWorkItem((CodifierExpandoObject)element);
                    cr.PipelineArtifactExecution = ctx;
                    QueueArtifactWorkItem(cr);
                }
                catch (Exception ex)
                {
                    // Exception intentionally swallowed; logging can be added if needed.
                }
            }
        }

        /// <summary>
        /// Logs the current counts of the data and collector result queues.
        /// </summary>
        protected override void LogQueueCount()
        {
            FEARHostedTelemetry.SendExecutionTelemetry($"Work Item Queue: {WorkItemQueue.Count}", TelemetryService);
        }

        /// <summary>
        /// This is the statistics for the current queue counts by stage.
        /// </summary>
        /// <returns></returns>
        public Dictionary<string, int> QueueCounts()
        {
            Dictionary<string, int> StageCounts = new Dictionary<string, int>();
            _Stages.Keys.ToList().ForEach(stage =>
            {
                if (!StageCounts.ContainsKey(stage))
                    StageCounts.Add(stage, WorkItemQueue.Count(t => t.CurrentStage == stage));
            });

            return StageCounts;
        }


        /// <summary>
        /// Processes items from the data and collector result queues, running collectors and codifiers as needed.
        /// </summary>
        /// <param name="maxDequeueSize">The maximum number of items to dequeue and process in one run.</param>
        /// <returns>The number of items processed (currently always 0).</returns>
        protected override int InternalRunQueue(int maxDequeueSize)
        {
            int count = 0;

            List<ArtifactWorkItem> localExecutionQueue = new List<ArtifactWorkItem>();

            for (int i = 0; i < maxDequeueSize; i++)
            {
                WorkItemQueue.TryDequeue(out ArtifactWorkItem dcr);
                if (dcr != null)
                    localExecutionQueue.Add(dcr);
            }

            // Process data queue items with collectors
            if (Parallel)
            {
                System.Threading.Tasks.Parallel.ForEach(localExecutionQueue, StageExecutor);
            }
            else
                localExecutionQueue.ForEach(StageExecutor);

            return count;
        }

        private void StageExecutor(ArtifactWorkItem workItem)
        {
            _Stages[workItem.CurrentStage](workItem);
            workItem.MarkStageComplete(workItem.CurrentStage);

            if (!workItem.IsComplete)
            {
                WorkItemQueue.Enqueue(workItem);
            }
        }

        private ArtifactWorkResult RunCollectorWorkItem(ArtifactWorkItem dcr)
        {
            List<ArtifactWorkItem> resultEntities = new List<ArtifactWorkItem>();
            try
            {
                // This should always be a single EntityResult as the post-processing of collectors
                // should have expanded EntityListResults into multiple EntityResults.
                EntityResult resultEntity = dcr.Entity as EntityResult;

                // Find the collectors that accept the collector result type
                List<IFEARCollector> gc = SearchForCollector(resultEntity.Entity, resultEntity.EntityTypeName);

                if (gc != null)
                {
                    // Execute each collector and collect results
                    gc.ForEach(collector =>
                    {
                        IExecutionServiceProvider exServices = CreateExecutionServiceProvider();
                        var rocc = new ResultObjectCollectorContext(collector, null, exServices);
                        ArtifactWorkItem collectorResult = rocc.Execute(dcr);
                        resultEntities.Add(collectorResult);
                    });
                }

                if (resultEntities.Count > 0)
                {
                    resultEntities.ForEach(t =>
                    {
                        if (t.Entity is EntityResult er)
                        {
                            var workItem = CreateArtifactWorkItem((CodifierExpandoObject)er.Entity);
                            workItem.PipelineArtifactExecution = dcr.PipelineArtifactExecution;
                            QueueArtifactWorkItem(workItem);
                        }
                        else if (t.Entity is EntityListResult elr)
                        {
                            var items = t.Entity as EntityListResult;
                            items?.Entities?.ForEach(te =>
                            {
                                var workItem = CreateArtifactWorkItem((CodifierExpandoObject)te);
                                workItem.PipelineArtifactExecution = dcr.PipelineArtifactExecution;
                                QueueArtifactWorkItem(workItem);
                            });
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                FEARHostedTelemetry.SendExecutionTelemetry($"Error: {ex.Message}", TelemetryService, FEARTelemetrySignalTypeEnum.Exception);
            }

            return null;
        }

        private ArtifactWorkResult RunCodifierWorkItem(ArtifactWorkItem cr)
        {
            try
            {
                List<CodifierExpandoObject> entities = new List<CodifierExpandoObject>();
                if (cr.Entity is EntityListResult)
                {
                    entities = ((EntityListResult)cr.Entity).Entities;
                }
                else
                {
                    entities.Add((CodifierExpandoObject)((EntityResult)cr.Entity).Entity);
                }

                // group by typename, or  type if typename not present. Check must be case insensitive
                IEnumerable<IGrouping<string, CodifierExpandoObject>> entitiesByTypeName = entities.GroupBy(t =>
                {
                    var dict = t.InternalValue;
                    var typeKey = dict.Keys.FirstOrDefault(k => k.Equals("typename", StringComparison.OrdinalIgnoreCase) || k.Equals("type", StringComparison.OrdinalIgnoreCase));
                    if (typeKey != null)
                        return (string)dict[typeKey];
                    return "";
                });

                foreach (var grouping in entitiesByTypeName)
                {
                    // Find the codifiers that accept the collector result type
                    foreach (CodifierExpandoObject resultEntity in grouping)
                    {
                        List<IFEARGraphCodifier> gcList = SearchForCodifier(resultEntity, grouping.Key);

                        if (gcList != null)
                        {
                            // Execute each codifier
                            gcList.ForEach(execCodifier);

                            void execCodifier(IFEARGraphCodifier codifier)
                            {
                                var ephemeralService = GraphManager.CreateGraphCodifyService();
                                var gcc = new GraphCodifierContext(codifier, ephemeralService);
                                gcc.Execute(resultEntity);

                                cr.PipelineArtifactExecution?.OnCompleted(ephemeralService.EphemeralGraph, cr.PipelineArtifactExecution);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                FEARHostedTelemetry.SendExecutionTelemetry($"Error: {ex.Message}", TelemetryService, FEARTelemetrySignalTypeEnum.Exception);
            }
            return null;
        }

        protected override int InternalRunRuleSet(int maxDequeueSize)
        {
            int count = 0;
            // Process up to maxDequeueSize items from the rulesets.
            maxDequeueSize = Math.Min(maxDequeueSize, Rulesets.Count);

            if (GraphManager.MaterializedGraph is IRemoteGraph remoteGraph)
            {
                var brg = remoteGraph as BaseRemoteGraph;
                if (brg.GraphLanguage != "SPARQL")
                {
                    FEARHostedTelemetry.SendExecutionTelemetry($"RuleSets are not compatible to the current graph language: {brg.GraphLanguage}. Skipping.", TelemetryService, FEARTelemetrySignalTypeEnum.Exception);
                    return 0;
                }
            }

            for (int i = 0; i < maxDequeueSize; i++)
            {
                if (Rulesets.Count > 0)
                {
                    IFEARRuleSet rs = GetNextRuleSet();

                    FEARHostedTelemetry.SendExecutionTelemetry($"Processing RuleSet: {rs.RulesetName} ({rs.RulesetCategory})", TelemetryService);

                    foreach (var rule in rs.Rules)
                    {
                        FEARHostedTelemetry.SendExecutionTelemetry($"Processing Rule: {rule.Key}", TelemetryService);
                        // This needs to be customised for Cypber, GraphQL, and SPARQL rules.

                        SparqlParameterizedString sparqlConstruct = new SparqlParameterizedString();
                        sparqlConstruct.CommandText = rs.ConstructQueryFor(rule.Value.SparqlConstructQuery);
                        // The construct query checks if the rule will produce any triples before executing it.
                        var result = GraphManager.MaterializedGraph.ExecuteQuery(sparqlConstruct);

                        // If we don't expect to construct any relations, continue to the next rule
                        if (result is VDS.RDF.Graph)
                        {
                            if ((result as VDS.RDF.Graph).IsEmpty)
                                continue;
                        }

                        SparqlParameterizedString sparqlInsert = new SparqlParameterizedString();
                        sparqlInsert.CommandText = rs.ConstructQueryFor(rule.Value.SparqlInsertQuery);
                        // The insert query applies the rule to the materialized graph.
                        GraphManager.MaterializedGraph.ExecuteUpdate(sparqlInsert);
                    }
                }
                count++;
            }
            return count;
        }

        private IFEARRuleSet GetNextRuleSet()
        {
            // Round-robin selection of the next rule set.
            if (curentRuleSetIndex >= Rulesets.Count)
            {
                curentRuleSetIndex = 0;
            }
            var ruleSet = Rulesets[curentRuleSetIndex];
            curentRuleSetIndex++;
            return ruleSet;
        }

        /// <summary>
        /// Converts a dynamic data object into a <see cref="ArtifactWorkItem"/> for processing.
        /// </summary>
        /// <param name="data">The dynamic data object.</param>
        /// <returns>A <see cref="ArtifactWorkItem"/> representing the data.</returns>
        protected ArtifactWorkItem CreateArtifactWorkItem(CodifierExpandoObject data)
        {
            try
            {
                var expandoDict = data.InternalValue;
                string typeName = "";
                var expandoKeys = expandoDict.Keys.ToList();
                foreach (var typeKey in new string[] { "type", "typename" })
                {
                    if (expandoKeys.Contains(typeKey, StringComparer.OrdinalIgnoreCase))
                    {
                        string key = expandoKeys.First(t => t.ToLower() == typeKey);
                        typeName = (string)expandoDict[key];
                        break;
                    }
                }

                ArtifactWorkItem cr = new ArtifactWorkItem(null, new EntityResult() { Entity = data, EntityTypeName = typeName }, typeName);
                return cr;
            }
            catch (System.Exception ex)
            {
                FEARHostedTelemetry.SendExecutionTelemetry($"Error creating ArtifactWorkItem: {ex.Message}", TelemetryService, FEARTelemetrySignalTypeEnum.Exception);
                throw ex;
            }
        }

        /// <summary>
        /// Mapping of property hashes to codifier type names for property-based codifier resolution.
        /// </summary>
        private Dictionary<string, List<string>> hashMapping = new Dictionary<string, List<string>>();

        /// <summary>
        /// Searches for collectors that can handle the specified result type.
        /// </summary>
        /// <param name="cr">The result object as a dictionary.</param>
        /// <param name="resultType">The type name of the result.</param>
        /// <returns>A list of matching collectors.</returns>
        protected List<IFEARCollector> SearchForCollector(CodifierExpandoObject cr, string resultType)
        {
            List<IFEARCollector> collectors = new List<IFEARCollector>();
            if (!string.IsNullOrEmpty(resultType))
            {
                foreach (var col in Collectors)
                {
                    if (col.DoesHandleResultObjectType(resultType))
                        collectors.Add(col);
                }
            }

            return collectors;
        }

        /// <summary>
        /// Searches for codifiers that can handle the specified accept type or, if not found, uses property-based matching.
        /// </summary>
        /// <param name="cr">The result object as a dictionary.</param>
        /// <param name="acceptType">The accept type to match.</param>
        /// <returns>A list of matching codifiers, or null if none found and strict matching is enabled.</returns>
        protected List<IFEARGraphCodifier> SearchForCodifier(CodifierExpandoObject cr, string acceptType)
        {
            if (!string.IsNullOrEmpty(acceptType))
            {
                if (GraphCodifiers.ContainsKey(acceptType))
                {
                    return GraphCodifiers[acceptType];
                }
            }

            if (ExecutionOptions.Arguments.TypeMatchingOption.Value == TypeMatchingEnum.Strict)
                return null;

            // Use the types on the object to determine the codifiers if no acceptType is supplied, or none matches
            List<string> properties = cr.InternalValue.Keys.Order().ToList();
            string propertiesHash = Convert.ToBase64String(hashAlgorithm.ComputeHash(System.Text.Encoding.UTF8.GetBytes(string.Join("", properties))));
            if (!hashMapping.ContainsKey(propertiesHash))
            {
                foreach (var item in GraphCodifiers)
                {
                    foreach (var item1 in item.Value)
                    {
                        int count = 0;

                        foreach (var propertyName in properties)
                        {
                            if (item1.AcceptsProperties.Contains(propertyName, StringComparer.OrdinalIgnoreCase))
                            {
                                count++;
                            }
                        }

                        var percent = count / (double)item1.AcceptsProperties.Length;
                        if (percent > 0.75)
                        {
                            if (!hashMapping.ContainsKey(propertiesHash))
                                hashMapping.Add(propertiesHash, new List<string>());

                            if (!hashMapping[propertiesHash].Contains(item.Key))
                                hashMapping[propertiesHash].Add(item.Key);
                        }
                    }
                }
            }

            if (hashMapping.ContainsKey(propertiesHash))
            {
                var typeList = hashMapping[propertiesHash];
                return typeList.SelectMany(x => GraphCodifiers[x]).ToList();
            }

            return null;
        }

        /// <summary>
        /// Provides a default host builder for FEAR execution contexts, configuring all required services.
        /// </summary>
        /// <typeparam name="TExecutionContext">The type of execution context to build.</typeparam>
        /// <param name="fearExecutionOptions">The execution options.</param>
        /// <param name="compilerOptions">The compiler options.</param>
        /// <returns>An <see cref="IHostApplicationBuilder"/> configured for FEAR execution.</returns>
        public static HostApplicationBuilder DefaultBuilder<TExecutionContext>(FearExecutionOptions<T> fearExecutionOptions, FEARCompilerOptions compilerOptions)
            where TExecutionContext : FearExecutionContext<T>
        {
            HostApplicationBuilder builder = Host.CreateApplicationBuilder();

            builder.Services.AddSingleton(ctx => compilerOptions);
            builder.Services.AddSingleton(ctx => fearExecutionOptions);
            builder.Services.AddSingleton<FearExecutionOptions>(ctx => ctx.GetService<FearExecutionOptions<T>>());
            builder.Services.AddSingleton(ctx => new FEARResolverOptions()
            {
                PreCompiledDirectory = fearExecutionOptions.Arguments.GetPreCompiledDirectory(),
                IgnorePreCompiledDirectory = fearExecutionOptions.Arguments.SourceCompilationOption.Value == SourceCompilationEnum.CompileScripts
            });

            builder.Services.AddSingleton<IFEARTelemetrySignalService, DefaultFEARTelemetrySignalService>();
            builder.Services.AddSingleton<IFEARTelemetryDestinationProvider>(ctx => new DefaultFEARTelemetryDestinationProvider(ctx.GetService<IConfiguration>(), Path.Combine(fearExecutionOptions.Arguments.GetWorkingDirectory(), "Logs")));
            builder.Services.AddSingleton<ITypeConversionService, DefaultTypeConversionService>();
            builder.Services.AddSingleton<IGraphCodifyServiceConfiguration>(ctx => new GraphCodifyServiceConfiguration() { StrictConversion = false });
            builder.Services.AddSingleton<IFEARResolver, BaseFEARResolver>();
            builder.Services.AddSingleton<IFEARRepositoryManager, FEARRepositoryManager>();
            builder.Services.AddSingleton<IToolResolver, FileToolResolver>();
            builder.Services.AddSingleton<IFEAROntologyStore, FEAROntologyStore>();
            builder.Services.AddSingleton<IFEARCompiler, FEARCompiler>();
            builder.Services.AddSingleton<ITemporaryDirectoryManager>(ctx => new TemporaryDirectoryManager(Path.Combine(fearExecutionOptions.Arguments.GetWorkingDirectory(), "temp"), true));
            builder.Services.AddSingleton(ctx => new GraphManagerConfiguration(ctx, null) { OntologyGraphFolder = "Ontologies" });
            builder.Services.AddSingleton(ctx => new Func<string, IIRIGenerator>(param => new IRIGenerator(param)));
            builder.Services.AddSingleton((ctx) => Environment.Build(ctx.GetService<IFEARCompiler>()));
            builder.Services.AddSingleton<IGraphManager, GraphManager>();
            builder.Services.AddSingleton<FindEntityStrategyFactoryManager>();
            builder.Services.AddSingleton<IGraphUriInfo>(ctx => new GraphUriInfo(fearExecutionOptions.Arguments.NamespaceOption.Value, fearExecutionOptions.Arguments.NamespaceAbbrevOption.Value));
            builder.Services.AddScoped<IMaterializedGraph, LocalGraph>();
            builder.Services.AddScoped<TExecutionContext>();
            builder.Services.AddLogging(cfg => cfg.AddConsole());

            builder.Services.AddTransient((ctx) => ctx.GetService<ILoggerFactory>().CreateLogger("FEAR"));
            builder.Services.AddTransient<IFindEntityStrategyFactory, DefaultFindEntityStrategyFactory>();

            fearExecutionOptions.ApplyDependencyInjection(builder);
            return builder;
        }
    }
}
