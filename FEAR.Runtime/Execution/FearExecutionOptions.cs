using FEAR.Domain.Arguments;
using FEAR.Domain.Extensions;
using FEAR.Domain.KnowledgeGraph.EntitySearch;
using FEAR.Domain.KnowledgeGraph.GraphDB;
using FEAR.Domain.KnowledgeGraph.GraphManager;
using FEAR.Domain.KnowledgeGraph.Graphs;
using FEAR.Runtime.Domain;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using VDS.RDF.Parsing;

namespace FEAR.Runtime.Execution
{
    /// <summary>
    /// Base class for FEAR execution options, providing configuration and argument access.
    /// </summary>
    public abstract class FearExecutionOptions
    {
        protected Lazy<IConfiguration?> _configuration;
        /// <summary>
        /// Lazily loaded configuration for the execution context.
        /// </summary>
        protected IConfiguration Configuration => _configuration.Value;

        /// <summary>
        /// Function to load the configuration when needed.
        /// </summary>
        protected Func<IConfiguration?> LoadConfigurationFunc { get; set; }

        /// <summary>
        /// Gets or sets the arguments for the execution context.
        /// </summary>
        public abstract FearArguments Arguments { get; protected set; }

        public string DefaultTelemetryServiceSource { get; protected set; }

        /// <summary>
        /// Gets or sets the case identifier (case number, or case name if number is absent)
        /// automatically stamped on every telemetry signal emitted through this execution context.
        /// </summary>
        public string CaseIdentifier { get; set; }
    }

    /// <summary>
    /// Generic execution options for FEAR, parameterized by the argument type.
    /// Handles dependency injection and configuration application for the execution context.
    /// </summary>
    /// <typeparam name="T">A type derived from <see cref="FearArguments"/>.</typeparam>
    public abstract class FearExecutionOptions<T> : FearExecutionOptions
        where T : FearArguments
    {
        /// <summary>
        /// Gets or sets the strongly-typed arguments for the execution context.
        /// </summary>
        public override FearArguments Arguments { get; protected set; }

        /// <summary>
        /// Gets the arguments as the specific type <typeparamref name="T"/>.
        /// </summary>
        public T TypedArguments => Arguments as T;

        /// <summary>
        /// Initializes a new instance of the <see cref="FearExecutionOptions{T}"/> class.
        /// </summary>
        /// <param name="arguments">The arguments for the execution context.</param>
        public FearExecutionOptions(T arguments, string defaultTelemetryServiceSource)
        {
            Arguments = arguments;
            _configuration = new Lazy<IConfiguration?>(() => LoadConfigurationFunc());
            DefaultTelemetryServiceSource = defaultTelemetryServiceSource;
        }

        /// <summary>
        /// Applies dependency injection and configuration to the provided host builder.
        /// Configures repository paths, graph manager, and entity strategy factories based on configuration.
        /// </summary>
        /// <param name="builder">The host builder to configure.</param>
        public void ApplyDependencyInjection(HostApplicationBuilder builder)
        {
            LoadAndApplyConfiguration(builder);

            if (Configuration == null)
                return;

            InitializeRepositoryPaths();

            ApplyServiceConfiguration(builder);
        }

        public void ApplyServiceConfiguration(HostApplicationBuilder builder)
        {
            ConfigureForRemoteGraphs(builder.Services);
            ConfigureCustomEntitySearchStrategies(builder.Services);
            ConfigureAgentOptions(builder.Services);

            PostApplyInjections(builder, Configuration);
        }

        private void ConfigureAgentOptions(IServiceCollection cfg)
        {
            if (Configuration == null)
                return;

            if (Configuration.GetSection("AgentOptions") != null)
            {
                // Load the Agent Adapter configurations
                cfg.AddSingleton<AgentAdapterRegistry>();
            }
        }

        /// <summary>
        /// Configures custom entity search strategies based on the provided configuration.
        /// </summary>
        /// <param name="cfg"></param>
        private void ConfigureCustomEntitySearchStrategies(IServiceCollection cfg)
        {
            if (Configuration == null)
                return;

            // Configure entity strategy factories if specified in configuration
            if (Configuration.GetSection("FindEntityStrategyFactory") != null)
            {
                var section = Configuration.GetSection("FindEntityStrategyFactory");

                var typeName = section.GetValue<string>("Local");
                if (!String.IsNullOrEmpty(typeName))
                {
                    var fesfType = Type.GetType(typeName);
                    if (fesfType != null)
                    {
                        var localStrategy = cfg.Single(t => t.ServiceType == typeof(IFindEntityStrategyFactory) && (t.ImplementationType?.IsAssignableTo(typeof(ILocalGraphFindEntityStrategyFactory)) ?? false));
                        cfg.Remove(localStrategy);
                        cfg.AddTransient<IFindEntityStrategyFactory>(ctx => ctx.CreateInstance<ILocalGraphFindEntityStrategyFactory>(fesfType));
                    }
                }
            }
        }

        /// <summary>
        /// Configures the host builder for remote graphs based on the provided configuration.
        /// </summary>
        /// <param name="cfg"></param>
        private void ConfigureForRemoteGraphs(IServiceCollection cfg)
        {
            if (Configuration == null)
                return;

            // Configure remote graph manager if specified in configuration
            if (Configuration.GetSection("GraphManager") != null)
            {
                var section = Configuration.GetSection("GraphManager");
                if (section.GetValue<bool>("RemoteGraph") == true)
                {
                    if (Configuration.GetSection("Connections:GraphDBConnection") != null)
                    {
                        cfg.RemoveAll<IGraphManager>();
                        cfg.RemoveAll<IMaterializedGraph>();
                        cfg.AddSingleton<IGraphManager, GraphManager>();

                        string graphSetupProvider = section.GetValue<string>("RemoteGraphSetupProvider") ?? "FEAR.Runtime.KnowledgeGraph.Sparql.FusekiGraphSetupProvider";
                        Type type = Type.GetType(graphSetupProvider);
                        if (type == null)
                        {
                            throw new ArgumentException($"Graph setup provider type {graphSetupProvider} not found.");
                        }

                        IGraphSetupProvider graphSetupProviderInstance = (IGraphSetupProvider)Activator.CreateInstance(type);
                        if (graphSetupProviderInstance == null)
                        {
                            throw new ArgumentException($"Graph setup provider {graphSetupProvider} could not be instantiated.");
                        }

                        graphSetupProviderInstance.ConfigureRemoteGraph(cfg, Configuration);
                    }
                }

                var typeName = section.GetValue<string>("Remote");
                if (!String.IsNullOrEmpty(typeName))
                {
                    var fesfType = Type.GetType(typeName);
                    if (fesfType != null)
                    {
                        var remoteStrategy = cfg.Single(t => t.ServiceType == typeof(IFindEntityStrategyFactory) && (t.ImplementationType?.IsAssignableTo(typeof(IRemoteGraphFindEntityStrategyFactory)) ?? false));
                        cfg.Remove(remoteStrategy);
                        cfg.AddTransient<IFindEntityStrategyFactory>(ctx => ctx.CreateInstance<IRemoteGraphFindEntityStrategyFactory>(fesfType));
                    }
                }
            }
        }

        /// <summary>
        /// Initializes repository paths from the configuration sections.
        /// </summary>
        private void InitializeRepositoryPaths()
        {
            // Map configuration sections to repository path parameters
            Dictionary<string, FearRepositoryPaths> argumentRepositoryPaths = new Dictionary<string, FearRepositoryPaths>()
                    {
                        { "Repositories", Arguments.SourceRepositoryPaths },
                        { "PrecompiledLibraries", Arguments.PrecompiledLibraryPaths },
                        { "Libraries", Arguments.PrecompiledLibraryPaths },
                        { "RestrictTo", Arguments.RestrictTo }
                    };

            if (Configuration.GetSection("PackagedSources") != null)
            {
                var section = Configuration.GetSection("PackagedSources");
                var values = section.Get<String[]>();
                if (values?.Length > 0)
                {
                    Arguments.PackagedSourcesPaths.AddRange(values);
                }
            }

            foreach (var repositoryType in argumentRepositoryPaths)
            {
                if (Configuration.GetSection(repositoryType.Key) != null)
                {
                    var section = Configuration.GetSection(repositoryType.Key);
                    var ifearPath = section.GetSection("IFEAR").Get<string[]>();
                    var cfearPath = section.GetSection("CFEAR").Get<string[]>();
                    var gfearPath = section.GetSection("GFEAR").Get<string[]>();
                    var rfearPath = section.GetSection("RFEAR").Get<string[]>();

                    if (ifearPath?.Length > 0)
                    {
                        if (!repositoryType.Value.IFEAR.IsSet)
                            repositoryType.Value.IFEAR = new Parameter<List<string>>(new List<string>(ifearPath));
                        else
                            repositoryType.Value.IFEAR.Value.AddRange(ifearPath);
                    }

                    if (cfearPath?.Length > 0)
                    {
                        if (!repositoryType.Value.CFEAR.IsSet)
                            repositoryType.Value.CFEAR = new Parameter<List<string>>(new List<string>(cfearPath));
                        else
                            repositoryType.Value.CFEAR.Value.AddRange(cfearPath);
                    }

                    if (gfearPath?.Length > 0)
                    {
                        if (!repositoryType.Value.GFEAR.IsSet)
                            repositoryType.Value.GFEAR = new Parameter<List<string>>(new List<string>(gfearPath));
                        else
                            repositoryType.Value.GFEAR.Value.AddRange(gfearPath);
                    }

                    if (rfearPath?.Length > 0)
                    {
                        if (!repositoryType.Value.RFEAR.IsSet)
                            repositoryType.Value.RFEAR = new Parameter<List<string>>(new List<string>(rfearPath));
                        else
                            repositoryType.Value.RFEAR.Value.AddRange(rfearPath);
                    }
                }
            }
        }

        /// <summary>
        /// Applies additional configuration to the host builder and updates argument options from configuration.
        /// </summary>
        /// <param name="builder">The host builder to configure.</param>
        /// <param name="configuration">The configuration to apply.</param>
        public HostApplicationBuilder ApplyConfiguration(HostApplicationBuilder builder, IConfiguration configuration)
        {
            var configShowTranspile = Configuration.GetSection("DisplayTranspiled").Get<string[]>();
            if (configShowTranspile != null)
            {
                TypedArguments.DisplayTranspileOption = new Parameter<string[]>(configShowTranspile);
            }

            var configTestCompile = Configuration.GetSection("TestCompile").Get<Boolean>();
            if (configTestCompile != null)
            {
                TypedArguments.TestCompileOption = new Parameter<bool>(configTestCompile);
            }

            var configOntologyOutputFormat = Configuration.GetSection("OntologyOutputFormat").Get<string>();
            if (configOntologyOutputFormat != null)
            {
                TypedArguments.OntologyOutputFormatOption = new Parameter<string>(configOntologyOutputFormat);
            }

            builder = PostApplyConfiguration(builder, configuration);

            return builder;
        }

        /// <summary>
        /// Loads the provided configuration into the host builder and applies any configured transformations
        /// when the configuration file and settings are present.
        /// </summary>
        /// <param name="builder">The host builder to configure.</param>
        public void LoadAndApplyConfiguration(HostApplicationBuilder builder)
        {
            if (Configuration != null)
            {
                builder.Configuration.AddConfiguration(Configuration);
                builder = ApplyConfiguration(builder, Configuration);
            }
        }

        /// <summary>
        /// Allows derived classes to apply additional configuration after the main configuration is applied.
        /// </summary>
        /// <param name="builder">The host builder to configure.</param>
        /// <param name="configuration">The configuration to apply.</param>
        public Func<HostApplicationBuilder, IConfiguration, HostApplicationBuilder> PostApplyConfiguration { get; set; } = (builder, configuration) => { return builder; };
        public Func<HostApplicationBuilder, IConfiguration, HostApplicationBuilder> PostApplyInjections { get; set; } = (builder, configuration) => { return builder; };
    }
}
