using FEAR.Domain.Collector;
using FEAR.Domain.Infrastructure;

namespace FEAR.Runtime.CFEAR.CollectorResults
{
    /// <summary>
    /// Base class for collectors that operate on directories within the FEAR framework.
    /// Provides the structure for implementing directory-based data collection logic.
    /// Inheritors must implement the logic for processing directory input and producing results.
    /// </summary>
    public abstract class DirectoryCollectorBase : CollectorBase, IFEARCollector<string>
    {
        /// <summary>
        /// The collector type for directory collectors.
        /// </summary>
        protected static CollectorTypeEnum _collectorType = CollectorTypeEnum.Directory;

        /// <summary>
        /// Gets the collector type, which is <see cref="CollectorTypeEnum.Directory"/> for this base class.
        /// </summary>
        public override CollectorTypeEnum CollectorType => _collectorType;

        /// <summary>
        /// Executes the collector using a generic context object, casting it to the appropriate directory context.
        /// </summary>
        /// <param name="context">The context object, expected to be of type <see cref="ICollectorContext{String}"/>.</param>
        public override void Execute(object context)
        {
            Execute((ICollectorContext<string>)context);
        }

        /// <summary>
        /// Executes the collector logic for a specific directory context.
        /// Must be implemented by derived classes to define directory processing behavior.
        /// </summary>
        /// <param name="context">The directory collector context.</param>
        public abstract void Execute(ICollectorContext<string> context);

        /// <summary>
        /// Initializes a new instance of the <see cref="DirectoryCollectorBase"/> class with the specified resolver.
        /// </summary>
        /// <param name="resolver">The FEAR resolver for component management.</param>
        public DirectoryCollectorBase(IFEARResolver resolver) : base(resolver)
        {
        }
    }
}
