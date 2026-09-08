using FEAR.Domain.Collector;
using FEAR.Domain.Infrastructure;
using FEAR.Domain.Interpreter;
using FEAR.Domain.Module;

namespace FEAR.Runtime.CFEAR.CollectorResults
{
    /// <summary>
    /// Provides base implementations for various types of FEAR collectors.
    /// Collectors are responsible for processing input data (files, streams, directories, etc.)
    /// and producing results within the FEAR framework. This base class manages module, interpreter,
    /// and collector registration and resolution, and enforces implementation of core collector properties and methods.
    /// </summary>
    public abstract class CollectorBase : IFEARCollector
    {
        /// <summary>
        /// Lazily resolves the <see cref="IFEARResolver"/> used for component discovery and registration.
        /// </summary>
        Lazy<IFEARResolver> lazyResolver = null;

        /// <summary>
        /// Gets the FEAR resolver for resolving modules, interpreters, and collectors.
        /// </summary>
        protected IFEARResolver Resolver => lazyResolver.Value;

        /// <summary>
        /// Index of registered FEAR modules by name.
        /// </summary>
        protected Dictionary<string, IFEARModule> moduleIndex = new Dictionary<string, IFEARModule>();

        /// <summary>
        /// Index of registered FEAR interpreters by name.
        /// </summary>
        protected Dictionary<string, IFEARInterpreter> interpreterIndex = new Dictionary<string, IFEARInterpreter>();

        /// <summary>
        /// Index of registered FEAR collectors by name.
        /// </summary>
        protected Dictionary<string, IFEARCollector> collectorIndex = new Dictionary<string, IFEARCollector>();

        /// <summary>
        /// Initializes a new instance of the <see cref="CollectorBase"/> class with the specified resolver.
        /// </summary>
        /// <param name="resolver">The FEAR resolver for component management.</param>
        public CollectorBase(IFEARResolver resolver)
        {
            lazyResolver = new Lazy<IFEARResolver>(() => resolver);
            InternalInitialise();
        }

        /// <summary>
        /// Executes the collector with the provided context.
        /// </summary>
        /// <param name="context">The context or input data for the collector.</param>
        public abstract void Execute(object context);

        /// <summary>
        /// Performs internal initialization logic for the collector.
        /// Must be implemented by derived classes.
        /// </summary>
        protected abstract void InternalInitialise();

        /// <summary>
        /// Gets the name of the collector.
        /// </summary>
        public abstract string CollectorName { get; }

        /// <summary>
        /// Gets the category of the collector.
        /// </summary>
        public abstract string CollectorCategory { get; }

        /// <summary>
        /// Gets the type of input the collector processes.
        /// </summary>
        public abstract CollectorTypeEnum CollectorType { get; }
    }
}
