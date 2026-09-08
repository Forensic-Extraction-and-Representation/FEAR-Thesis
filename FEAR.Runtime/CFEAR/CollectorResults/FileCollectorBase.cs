using FEAR.Domain.Collector;
using FEAR.Domain.Infrastructure;

namespace FEAR.Runtime.CFEAR.CollectorResults
{
    /// <summary>
    /// Base class for collectors that operate on files within the FEAR framework.
    /// Provides the structure for implementing file-based data collection logic.
    /// Inheritors must implement the logic for processing file input and producing results.
    /// </summary>
    public abstract class FileCollectorBase : CollectorBase, IFEARCollector<string>
    {
        /// <summary>
        /// The collector type for file collectors.
        /// </summary>
        protected static CollectorTypeEnum _collectorType = CollectorTypeEnum.File;

        /// <summary>
        /// Gets the collector type, which is <see cref="CollectorTypeEnum.File"/> for this base class.
        /// </summary>
        public override CollectorTypeEnum CollectorType => _collectorType;

        /// <summary>
        /// Executes the collector using a generic context object, casting it to the appropriate file context.
        /// </summary>
        /// <param name="context">The context object, expected to be of type <see cref="ICollectorContext{String}"/>.</param>
        public override void Execute(object context)
        {
            Execute((ICollectorContext<string>)context);
        }

        /// <summary>
        /// Executes the collector logic for a specific file context.
        /// Must be implemented by derived classes to define file processing behavior.
        /// </summary>
        /// <param name="context">The file collector context.</param>
        public abstract void Execute(ICollectorContext<string> context);

        /// <summary>
        /// Initializes a new instance of the <see cref="FileCollectorBase"/> class with the specified resolver.
        /// </summary>
        /// <param name="resolver">The FEAR resolver for component management.</param>
        public FileCollectorBase(IFEARResolver resolver) : base(resolver)
        {
        }
    }
}
