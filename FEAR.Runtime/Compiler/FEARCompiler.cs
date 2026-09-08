using FEAR.Domain.Collector;
using FEAR.Domain.Extensions;
using FEAR.Domain.GraphCodifier;
using FEAR.Domain.Infrastructure;
using FEAR.Domain.Interpreter;
using FEAR.Domain.Module;
using FEAR.Domain.Ontology;
using FEAR.Domain.RuleSet;
using FEAR.Domain.Telemetry;
using FEAR.Runtime.Domain;
using FEAR.Runtime.Execution;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace FEAR.Runtime.Compiler
{
    /// <summary>
    /// The main compiler class for the FEAR framework.
    /// Responsible for orchestrating the discovery, compilation, and registration of collectors, interpreters,
    /// graph codifiers, and rulesets. Handles repository synchronization, assembly loading, and dependency resolution.
    /// </summary>
    public class FEARCompiler : IFEARCompiler
    {
        // Lazy initialization for core FEAR framework services
        Lazy<IFEARResolver> resolver = null;
        Lazy<IFEARCollectorSearcherContainer> searcher = null;
        Lazy<IFEARRepositoryManager> repoManager = null;

        /// <summary>
        /// Provides access to the collector searcher container for path-based collector resolution.
        /// </summary>
        public IFEARCollectorSearcherContainer CollectorSearcherContainer => searcher.Value;

        /// <summary>
        /// Provides access to the FEAR resolver for dynamic component registration and lookup.
        /// </summary>
        public IFEARResolver Resolver => resolver.Value;

        /// <summary>
        /// Provides access to the repository manager for synchronizing repositories and libraries.
        /// </summary>
        public IFEARRepositoryManager RepositoryManager => repoManager.Value;

        /// <summary>
        /// The dependency injection container for resolving services and creating instances.
        /// </summary>
        protected IServiceProvider Container { get; }

        protected IFEARTelemetryDestinationProvider TelemetryProvider { get; }
        protected IFEARTelemetryDestination TelemetryDestination { get; }
        protected IFEARTelemetrySignalService TelemetrySignalService { get; }

        /// <summary>
        /// The compiler options used for configuring compilation and source discovery.
        /// </summary>
        public FEARCompilerOptions Options { get; }

        /// <summary>
        /// The execution options used for runtime configuration and argument restriction.
        /// </summary>
        public FearExecutionOptions ExecutionOptions { get; }

        public const string COMPILER_SOURCE = "FEARCompiler";

        /// <summary>
        /// Initializes a new instance of the <see cref="FEARCompiler"/> class.
        /// Sets up repository synchronization, component registration, and triggers compilation as needed.
        /// </summary>
        /// <param name="ctx">The dependency injection container.</param>
        /// <param name="options">Compiler options for source and output configuration.</param>
        /// <param name="executionOptions">Execution options for runtime argument restriction.</param>
        public FEARCompiler(IServiceProvider ctx, IFEARTelemetryDestinationProvider telemetryProvider, FEARCompilerOptions options, FearExecutionOptions executionOptions)
        {
            Container = ctx;
            Options = options;
            ExecutionOptions = executionOptions;
            TelemetryProvider = telemetryProvider;
            TelemetrySignalService = Container.GetService<IFEARTelemetrySignalService>();
            TelemetryDestination = TelemetryProvider.RegisterDestination("LogFile", COMPILER_SOURCE,
                new Dictionary<string, object> {
                    { "LogFileName", $"fearcompiler-{DateTime.UtcNow:yyyyMMddHHmm}.log" }
                });

            // Ensure the compiled output directory exists
            if (!Directory.Exists(Options.CompiledDirectory))
                Directory.CreateDirectory(Options.CompiledDirectory);

            repoManager = new Lazy<IFEARRepositoryManager>(() => Container.GetService<IFEARRepositoryManager>());
            resolver = new Lazy<IFEARResolver>(() => ConstructResolver(Container.GetService<IFEARResolver>()));
            searcher = new Lazy<IFEARCollectorSearcherContainer>(() => new BaseFEARSearcherContainer());

            // Synchronize repositories and precompiled libraries
            foreach (var type in new List<string> { "CFEAR", "GFEAR", "RFEAR" })
            {
                repoManager.Value.SynchronizeRepository(type);
                repoManager.Value.SynchronizePrecompiledLibraries(type);
            }

            repoManager.Value.SynchronizePackageLibraries();

            // Trigger compilation for each FEAR component type as configured
            if (Options.CFEAROption.Compile)
                CompileAllCollectors();

            if (Options.IFEAROption.Compile)
                CompileAllInterpreters();

            if (Options.GFEAROption.Compile)
                CompileAllGraphCodifiers();

            if (Options.RFEAROption.Compile)
                CompileAllRuleSets();
        }

        /// <summary>
        /// Compiles all source files for a given FEAR component type using the provided options and restriction list.
        /// </summary>
        private void CompileAllOf(FEARSourceOption option, FEAR.Domain.Arguments.Parameter<List<string>> restrictList, Action<string, string> compilerAction)
        {
            if (Directory.Exists(option.Directory))
            {
                var files = Directory.GetFiles(option.Directory, "*.*").Where(x => option.FileExtensions.Contains(Path.GetExtension(x))).ToList();
                files.AddRange(Directory.GetDirectories(option.Directory).ToList());

                foreach (var file in files)
                {
                    string fileName = Path.GetFileName(file);

                    if (!restrictList.IsSet || restrictList.Value.Contains(fileName))
                    {
                        string outFile = Path.Combine(Options.CompiledDirectory, fileName + "." + option.FEARLanguage + ".dll");
                        compilerAction(file, outFile);
                    }
                }
            }
        }

        /// <summary>
        /// Compiles all interpreter source files.
        /// </summary>
        private void CompileAllInterpreters()
        {
            CompileAllOf(Options.IFEAROption.SourceOption, ExecutionOptions.Arguments.RestrictTo.IFEAR, CompileFEARSource<IFEARInterpreter>);
        }

        /// <summary>
        /// Compiles all collector source files.
        /// </summary>
        private void CompileAllCollectors()
        {
            CompileAllOf(Options.CFEAROption.SourceOption, ExecutionOptions.Arguments.RestrictTo.CFEAR, CompileFEARSource<IFEARCollector>);
        }

        /// <summary>
        /// Compiles all graph codifier source files.
        /// </summary>
        private void CompileAllGraphCodifiers()
        {
            CompileAllOf(Options.GFEAROption.SourceOption, ExecutionOptions.Arguments.RestrictTo.GFEAR, CompileFEARSource<IFEARGraphCodifier>);
        }

        /// <summary>
        /// Compiles all ruleset source files.
        /// </summary>
        private void CompileAllRuleSets()
        {
            CompileAllOf(Options.RFEAROption.SourceOption, ExecutionOptions.Arguments.RestrictTo.RFEAR, CompileFEARSource<IFEARRuleSet>);
        }

        /// <summary>
        /// Discovers and registers FEAR modules, interpreters, collectors, and graph codifiers from loaded assemblies.
        /// Uses custom attributes to identify and instantiate components, and registers them with the resolver.
        /// </summary>
        /// <param name="fr">The FEAR resolver to register components with.</param>
        /// <returns>The updated FEAR resolver.</returns>
        private IFEARResolver ConstructResolver(IFEARResolver fr)
        {
            foreach (var assemblyList in new List<List<Assembly>> { AppDomain.CurrentDomain.GetAssemblies().ToList() })
            {
                // Register FEAR modules
                if (Options.CFEAROption.Compile)
                {
                    assemblyList.SelectMany(x => x.GetTypes())
                        .Where(x => x.GetCustomAttributes(typeof(FEARModuleAttribute), false).Length > 0)
                        .ToList()
                        .ForEach(x =>
                        {
                            if (!ExecutionOptions.Arguments.RestrictTo.CFEAR.IsSet || ExecutionOptions.Arguments.RestrictTo.CFEAR.Value.Contains(Path.GetFileNameWithoutExtension(x.Assembly.CodeBase)))
                            {
                                var instance = Container.CreateInstance<IFEARModule>(x);
                                var attribute = x.GetCustomAttributes(typeof(FEARModuleAttribute), false).FirstOrDefault() as FEARModuleAttribute;
                                fr.AddModule(attribute, instance);
                                TelemetrySignalService.SendSignal(new GenericFEARTelemetrySignal(COMPILER_SOURCE, FEARTelemetrySerializationEnum.Raw, FEARTelemetrySignalTypeEnum.Environment).WithSignalData($"Registered FEAR Module: {attribute.FullName}"));
                            }
                        });
                }

                // Register FEAR interpreters
                if (Options.IFEAROption.Compile)
                {
                    assemblyList.SelectMany(x => x.GetTypes())
                        .Where(x => x.GetCustomAttributes(typeof(FEARInterpreterAttribute), false).Length > 0)
                        .ToList()
                        .ForEach(x =>
                        {
                            if (!ExecutionOptions.Arguments.RestrictTo.IFEAR.IsSet || ExecutionOptions.Arguments.RestrictTo.IFEAR.Value.Contains(Path.GetFileNameWithoutExtension(x.Assembly.CodeBase)))
                            {
                                var instance = Container.CreateInstance<IFEARInterpreter>(x);
                                var attribute = x.GetCustomAttributes(typeof(FEARInterpreterAttribute), false).FirstOrDefault() as FEARInterpreterAttribute;
                                fr.AddInterpreter(attribute, instance);
                                TelemetrySignalService.SendSignal(new GenericFEARTelemetrySignal(COMPILER_SOURCE, FEARTelemetrySerializationEnum.Raw, FEARTelemetrySignalTypeEnum.Environment).WithSignalData($"Registered FEAR Interpreter: {attribute.FullName}"));
                            }
                        });
                }

                // Register FEAR collectors and their searchers
                if (Options.CFEAROption.Compile)
                {
                    assemblyList.SelectMany(x => x.GetTypes())
                        .Where(x => x.GetCustomAttributes(typeof(FEARCollectorAttribute), false).Length > 0)
                        .ToList()
                        .ForEach(x =>
                        {
                            if (!ExecutionOptions.Arguments.RestrictTo.CFEAR.IsSet || ExecutionOptions.Arguments.RestrictTo.CFEAR.Value.Contains(Path.GetFileNameWithoutExtension(x.Assembly.CodeBase)))
                            {
                                var instance = Container.CreateInstance<IFEARCollector>(x);
                                var attribute = x.GetCustomAttributes(typeof(FEARCollectorAttribute), false).FirstOrDefault() as FEARCollectorAttribute;
                                fr.AddCollector(attribute, instance);

                                TelemetrySignalService.SendSignal(new GenericFEARTelemetrySignal(COMPILER_SOURCE, FEARTelemetrySerializationEnum.Raw, FEARTelemetrySignalTypeEnum.Environment).WithSignalData($"Registered FEAR Collector: {attribute.FullName}"));

                                // Register nested collector searchers
                                x.GetNestedTypes()
                                    .Where(y => y.GetCustomAttributes(typeof(CollectorSearchAttribute), false).Length > 0)
                                    .ToList()
                                    .ForEach(y =>
                                    {
                                        var searcherInstance = Container.CreateInstance<ICollectorSearch>(y);
                                        var searchAttribute = y.GetCustomAttributes(typeof(CollectorSearchAttribute), false).FirstOrDefault() as CollectorSearchAttribute;
                                        if (searchAttribute.For == x.GetType())
                                        {
                                            searcher.Value.AddSearcherForCollector(attribute, searcherInstance);
                                            TelemetrySignalService.SendSignal(new GenericFEARTelemetrySignal(COMPILER_SOURCE, FEARTelemetrySerializationEnum.Raw, FEARTelemetrySignalTypeEnum.Environment).WithSignalData($"Registered Collector Searcher: {attribute.FullName} for: {searchAttribute.For.FullName}"));
                                        }
                                    });
                            }
                        });
                }

                // Register FEAR graph codifiers
                if (Options.GFEAROption.Compile)
                {
                    assemblyList.SelectMany(x => x.GetTypes())
                        .Where(x => x.GetCustomAttributes(typeof(FEARGraphCodifierAttribute), false).Length > 0)
                        .ToList()
                        .ForEach(x =>
                        {
                            if (!ExecutionOptions.Arguments.RestrictTo.GFEAR.IsSet || ExecutionOptions.Arguments.RestrictTo.GFEAR.Value.Contains(Path.GetFileNameWithoutExtension(x.Assembly.CodeBase)))
                            {
                                var instance = Container.CreateInstance<IFEARGraphCodifier>(x);
                                var attribute = x.GetCustomAttributes(typeof(FEARGraphCodifierAttribute), false).FirstOrDefault() as FEARGraphCodifierAttribute;
                                fr.AddGraphCodifier(attribute, instance);
                                TelemetrySignalService.SendSignal(new GenericFEARTelemetrySignal(COMPILER_SOURCE, FEARTelemetrySerializationEnum.Raw, FEARTelemetrySignalTypeEnum.Environment).WithSignalData($"Registered FEAR Graph Codifier: {attribute.FullName}"));
                            }
                        });
                }

                // Register FEAR rulesets
                if (Options.RFEAROption.Compile)
                {
                    assemblyList.SelectMany(x => x.GetTypes())
                        .Where(x => x.GetCustomAttributes(typeof(FEARRuleSetAttribute), false).Length > 0)
                        .ToList()
                        .ForEach(x =>
                        {
                            if (!ExecutionOptions.Arguments.RestrictTo.RFEAR.IsSet || ExecutionOptions.Arguments.RestrictTo.RFEAR.Value.Contains(Path.GetFileNameWithoutExtension(x.Assembly.CodeBase)))
                            {
                                var instance = Container.CreateInstance<IFEARRuleSet>(x);
                                var attribute = x.GetCustomAttributes(typeof(FEARRuleSetAttribute), false).FirstOrDefault() as FEARRuleSetAttribute;
                                fr.AddRuleSet(attribute, instance);
                                TelemetrySignalService.SendSignal(new GenericFEARTelemetrySignal(COMPILER_SOURCE, FEARTelemetrySerializationEnum.Raw, FEARTelemetrySignalTypeEnum.Environment).WithSignalData($"Registered FEAR RuleSet: {attribute.RulesetName}"));
                            }
                        });
                }
            }

            return fr;
        }

        /// <summary>
        /// Returns the appropriate compile action for the given FEAR component type.
        /// </summary>
        private Action<FEARCompilationRequest> CompilerActionFor<CType>()
        {
            Type t = typeof(CType);
            if (t == typeof(IFEARCollector))
                return CollectorCompiler.Compile;
            if (t == typeof(IFEARInterpreter))
                return InterpreterCompiler.Compile;
            if (t == typeof(IFEARGraphCodifier))
                return GraphCodifierCompiler.Compile;
            if (t == typeof(IFEARRuleSet))
                return RuleSetCompiler.Compile;

            return null;
        }

        /// <summary>
        /// Compiles a FEAR source file of the specified type and outputs the resulting assembly.
        /// </summary>
        /// <typeparam name="T">The FEAR component type (e.g., collector, interpreter).</typeparam>
        /// <param name="filename">The source file to compile.</param>
        /// <param name="outFile">The output file path for the compiled assembly.</param>
        public void CompileFEARSource<T>(string filename, string outFile)
        {
            Action<FEARCompilationRequest> compileAction = CompilerActionFor<T>();
            compileAction(new FEARCompilationRequest(Options, ExecutionOptions, Container.GetService<IFEAROntologyStore>(), TelemetrySignalService, filename, outFile));

            // Load the .NET Assembly from the compile memory stream
            if (!Path.IsPathFullyQualified(outFile))
                outFile = Path.Combine(System.Environment.CurrentDirectory, outFile);

            TelemetrySignalService.SendSignal(new GenericFEARTelemetrySignal(COMPILER_SOURCE, FEARTelemetrySerializationEnum.Raw, FEARTelemetrySignalTypeEnum.Environment).WithSignalData($"Compiled FEAR Source: {Path.GetFileName(filename)} to {outFile}"));
            if (File.Exists(outFile))
            {
                Resolver.ResolveTypesInAssembly(outFile);
                TelemetrySignalService.SendSignal(new GenericFEARTelemetrySignal(COMPILER_SOURCE, FEARTelemetrySerializationEnum.Raw, FEARTelemetrySignalTypeEnum.Environment).WithSignalData($"Loaded Compiled Assembly into Resolver: {Path.GetFileName(outFile)}"));
            }
        }
    }
}
