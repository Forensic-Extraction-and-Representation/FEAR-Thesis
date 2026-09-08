using FEAR.Domain.Collector;
using FEAR.Domain.GraphCodifier;
using FEAR.Domain.Interpreter;
using FEAR.Domain.Module;
using FEAR.Domain.RuleSet;
using Neo4j.Driver;
using System.Reflection;
using System.Runtime.Loader;

namespace FEAR.Domain.Infrastructure
{
    /// <summary>
    /// Provides a base implementation of the <see cref="IFEARResolver"/> interface for the FEAR framework.
    /// This resolver is responsible for discovering, loading, and registering FEAR modules, interpreters, collectors,
    /// and graph codifiers from precompiled assemblies. It maintains indexes for each type, allowing for efficient
    /// resolution and retrieval by name.
    ///
    /// The resolver uses custom attributes (<see cref="FEARModuleAttribute"/>, <see cref="FEARInterpreterAttribute"/>,
    /// <see cref="FEARCollectorAttribute"/>, <see cref="FEARGraphCodifierAttribute"/>) to associate metadata and
    /// registration information with each discovered type. This enables dynamic extensibility and plugin support.
    /// </summary>
    public class BaseFEARResolver : IFEARResolver, IDisposable
    {
        AssemblyLoadContext AssemblyLoadContext { get; set; }
        // Internal indexes for fast lookup by full name
        Dictionary<string, IFEARModule> moduleIndex = new Dictionary<string, IFEARModule>();
        Dictionary<string, IFEARInterpreter> interpreterIndex = new Dictionary<string, IFEARInterpreter>();
        Dictionary<string, IFEARCollector> collectorIndex = new Dictionary<string, IFEARCollector>();
        Dictionary<string, IFEARGraphCodifier> graphCodifierIndex = new Dictionary<string, IFEARGraphCodifier>();
        Dictionary<string, IFEARRuleSet> ruleSetIndex = new Dictionary<string, IFEARRuleSet>();

        /// <summary>
        /// Gets the list of registered FEAR modules.
        /// </summary>
        public IList<IFEARModule> Modules => moduleIndex.Values.ToList();

        /// <summary>
        /// Gets the list of registered FEAR interpreters.
        /// </summary>
        public IList<IFEARInterpreter> Interpreters => interpreterIndex.Values.ToList();

        /// <summary>
        /// Gets the list of registered FEAR collectors.
        /// </summary>
        public IList<IFEARCollector> Collectors => collectorIndex.Values.ToList();

        /// <summary>
        /// Gets the list of registered FEAR graph codifiers.
        /// </summary>
        public IList<IFEARGraphCodifier> GraphCodifiers => graphCodifierIndex.Values.ToList();

        public IList<IFEARRuleSet> RuleSets => ruleSetIndex.Values.ToList();

        /// <summary>
        /// Initializes a new instance of the <see cref="BaseFEARResolver"/> class.
        /// Discovers and loads modules, interpreters, collectors, and graph codifiers from precompiled assemblies
        /// in the specified directory, unless <c>IgnorePreCompiledDirectory</c> is set.
        /// </summary>
        /// <param name="options">Resolver options specifying the precompiled directory and loading behavior.</param>
        public BaseFEARResolver(FEARResolverOptions options)
        {
            AssemblyLoadContext = new AssemblyLoadContext("FEARResolverContext", isCollectible: true);

            if (!options.IgnorePreCompiledDirectory && Directory.Exists(options.PreCompiledDirectory))
            {
                // Get all assemblies in the precompiled directory and subdirectories
                var assemblies = Directory.GetFiles(options.PreCompiledDirectory, "*.dll", SearchOption.AllDirectories);

                // Load the assembly into the current application domain
                foreach (var assembly in assemblies)
                {
                    ResolveTypesInAssembly(assembly);
                }
            }
        }

        public void ResolveTypesInAssembly(string assembly)
        {
            var precompiledAssembly = AssemblyLoadContext.LoadFromStream(new MemoryStream(File.ReadAllBytes(assembly)));
            //var precompiledAssembly = Assembly.LoadFrom(assembly);

            try
            {
                // Register all types implementing IFEARModule
                foreach (var type in precompiledAssembly.GetTypes().Where(t => typeof(IFEARModule).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract))
                {
                    var instance = (IFEARModule)Activator.CreateInstance(type);
                    AddModule(type.GetCustomAttributes<FEARModuleAttribute>().First(), instance);
                }

                // Register all types implementing IFEARInterpreter
                foreach (var type in precompiledAssembly.GetTypes().Where(t => typeof(IFEARInterpreter).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract))
                {
                    var instance = (IFEARInterpreter)Activator.CreateInstance(type);
                    AddInterpreter(type.GetCustomAttributes<FEARInterpreterAttribute>().First(), instance);
                }

                // Register all types implementing IFEARCollector
                foreach (var type in precompiledAssembly.GetTypes().Where(t => typeof(IFEARCollector).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract))
                {
                    var instance = (IFEARCollector)Activator.CreateInstance(type, this);
                    AddCollector(type.GetCustomAttributes<FEARCollectorAttribute>().First(), instance);
                }

                // Register all types implementing IFEARGraphCodifier
                foreach (var type in precompiledAssembly.GetTypes().Where(t => typeof(IFEARGraphCodifier).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract))
                {
                    var instance = (IFEARGraphCodifier)Activator.CreateInstance(type);
                    AddGraphCodifier(type.GetCustomAttributes<FEARGraphCodifierAttribute>().First(), instance);
                }

                // Register all types implementing IFEARRuleSet
                foreach (var type in precompiledAssembly.GetTypes().Where(t => typeof(IFEARRuleSet).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract))
                {
                    var instance = (IFEARRuleSet)Activator.CreateInstance(type);
                    AddRuleSet(type.GetCustomAttributes<FEARRuleSetAttribute>().First(), instance);
                }
            }
            catch (Exception ex)
            {

            }
        }

        public void Dispose()
        {
            AssemblyLoadContext.Unload();
        }

        /// <summary>
        /// Registers a FEAR module with the resolver.
        /// </summary>
        /// <param name="moduleAttribute">The module attribute containing registration metadata.</param>
        /// <param name="instance">The module instance to register.</param>
        public void AddModule(FEARModuleAttribute moduleAttribute, IFEARModule instance)
        {
            moduleIndex.TryAdd(moduleAttribute.FullName, instance);
        }

        /// <summary>
        /// Registers a FEAR interpreter with the resolver.
        /// </summary>
        /// <param name="interpreterAttribute">The interpreter attribute containing registration metadata.</param>
        /// <param name="instance">The interpreter instance to register.</param>
        public void AddInterpreter(FEARInterpreterAttribute interpreterAttribute, IFEARInterpreter instance)
        {
            interpreterIndex.TryAdd(interpreterAttribute.FullName, instance);
        }

        /// <summary>
        /// Registers a FEAR collector with the resolver.
        /// </summary>
        /// <param name="collectorAttribute">The collector attribute containing registration metadata.</param>
        /// <param name="instance">The collector instance to register.</param>
        public void AddCollector(FEARCollectorAttribute collectorAttribute, IFEARCollector instance)
        {
            collectorIndex.TryAdd(collectorAttribute.FullName, instance);
        }

        /// <summary>
        /// Registers a FEAR graph codifier with the resolver.
        /// </summary>
        /// <param name="graphCodifierAttribute">The graph codifier attribute containing registration metadata.</param>
        /// <param name="instance">The graph codifier instance to register.</param>
        public void AddGraphCodifier(FEARGraphCodifierAttribute graphCodifierAttribute, IFEARGraphCodifier instance)
        {
            graphCodifierIndex.TryAdd(graphCodifierAttribute.FullName, instance);
        }

        public void AddRuleSet(FEARRuleSetAttribute ruleSetAttribute, IFEARRuleSet instance)
        {
            ruleSetIndex.TryAdd(ruleSetAttribute.RulesetName, instance);
        }

        /// <summary>
        /// Resolves a registered FEAR module by name.
        /// </summary>
        /// <param name="moduleName">The full name of the module to resolve.</param>
        /// <returns>The resolved module instance, or null if not found.</returns>
        public IFEARModule ResolveModule(string moduleName)
        {
            if (moduleIndex.ContainsKey(moduleName))
                return moduleIndex[moduleName];
            else
                return null;
        }

        /// <summary>
        /// Resolves a registered FEAR interpreter by name.
        /// </summary>
        /// <param name="interpreterName">The full name of the interpreter to resolve.</param>
        /// <returns>The resolved interpreter instance, or null if not found.</returns>
        public IFEARInterpreter ResolveInterpreter(string interpreterName)
        {
            if (interpreterIndex.ContainsKey(interpreterName))
                return interpreterIndex[interpreterName];
            else
                return null;
        }

        /// <summary>
        /// Resolves a registered FEAR collector by name.
        /// </summary>
        /// <param name="collectorName">The full name of the collector to resolve.</param>
        /// <returns>The resolved collector instance, or null if not found.</returns>
        public IFEARCollector ResolveCollector(string collectorName)
        {
            if (collectorIndex.ContainsKey(collectorName))
                return collectorIndex[collectorName];
            else
                return null;
        }
    }
}
