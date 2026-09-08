using FEAR.Domain.Collector;
using FEAR.Domain.GraphCodifier;
using FEAR.Domain.Interpreter;
using FEAR.Domain.Module;
using FEAR.Domain.RuleSet;

namespace FEAR.Domain.Infrastructure
{
    /// <summary>
    /// Defines a contract for resolving and managing FEAR framework components, 
    /// including modules, interpreters, collectors, graph codifiers, and rulesets. 
    /// This interface supports dynamic discovery, registration, and retrieval of 
    /// these components by name or attribute, enabling extensibility and modularity 
    /// in the FEAR ecosystem.
    /// </summary>
    public interface IFEARResolver
    {
        /// <summary>
        /// Gets the list of registered FEAR modules.
        /// </summary>
        IList<IFEARModule> Modules { get; }

        /// <summary>
        /// Gets the list of registered FEAR interpreters.
        /// </summary>
        IList<IFEARInterpreter> Interpreters { get; }

        /// <summary>
        /// Gets the list of registered FEAR collectors.
        /// </summary>
        IList<IFEARCollector> Collectors { get; }

        /// <summary>
        /// Gets the list of registered FEAR graph codifiers.
        /// </summary>
        IList<IFEARGraphCodifier> GraphCodifiers { get; }

        /// <summary>
        /// Gets the list of registered FEAR rulesets.
        /// </summary>
        IList<IFEARRuleSet> RuleSets { get; }

        /// <summary>
        /// Resolves a registered FEAR module by name.
        /// </summary>
        /// <param name="v">The full name of the module to resolve.</param>
        /// <returns>The resolved module instance, or null if not found.</returns>
        IFEARModule ResolveModule(string v);

        /// <summary>
        /// Resolves a registered FEAR interpreter by name.
        /// </summary>
        /// <param name="v">The full name of the interpreter to resolve.</param>
        /// <returns>The resolved interpreter instance, or null if not found.</returns>
        IFEARInterpreter ResolveInterpreter(string v);

        /// <summary>
        /// Resolves a registered FEAR collector by name.
        /// </summary>
        /// <param name="v">The full name of the collector to resolve.</param>
        /// <returns>The resolved collector instance, or null if not found.</returns>
        IFEARCollector ResolveCollector(string v);

        /// <summary>
        /// Registers a FEAR module with the resolver.
        /// </summary>
        /// <param name="moduleAttribute">The module attribute containing registration metadata.</param>
        /// <param name="instance">The module instance to register.</param>
        void AddModule(FEARModuleAttribute moduleAttribute, IFEARModule instance);

        /// <summary>
        /// Registers a FEAR interpreter with the resolver.
        /// </summary>
        /// <param name="interpreterAttribute">The interpreter attribute containing registration metadata.</param>
        /// <param name="instance">The interpreter instance to register.</param>
        void AddInterpreter(FEARInterpreterAttribute interpreterAttribute, IFEARInterpreter instance);

        /// <summary>
        /// Registers a FEAR collector with the resolver.
        /// </summary>
        /// <param name="collectorAttribute">The collector attribute containing registration metadata.</param>
        /// <param name="instance">The collector instance to register.</param>
        void AddCollector(FEARCollectorAttribute collectorAttribute, IFEARCollector instance);

        /// <summary>
        /// Registers a FEAR graph codifier with the resolver.
        /// </summary>
        /// <param name="codifierAttribute">The graph codifier attribute containing registration metadata.</param>
        /// <param name="instance">The graph codifier instance to register.</param>
        void AddGraphCodifier(FEARGraphCodifierAttribute codifierAttribute, IFEARGraphCodifier instance);

        /// <summary>
        /// Registers a FEAR ruleset with the resolver.
        /// </summary>
        /// <param name="ruleSetAttribute">The ruleset attribute containing registration metadata.</param>
        /// <param name="instance">The ruleset instance to register.</param>
        void AddRuleSet(FEARRuleSetAttribute ruleSetAttribute, IFEARRuleSet instance);

        /// <summary>
        /// Resolves and registers types from a specified assembly file.
        /// </summary>
        /// <param name="asssemblyPath">The file path of the assembly to load and resolve types from.</param>
        /// <returns>void</returns>
        void ResolveTypesInAssembly(string asssemblyPath);
    }
}
