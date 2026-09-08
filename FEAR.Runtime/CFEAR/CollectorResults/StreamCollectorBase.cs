using FEAR.Domain.Collector;
using FEAR.Domain.Infrastructure;

namespace FEAR.Runtime.CFEAR.CollectorResults
{
    /// <summary>
    /// Base class for collectors that operate on streams within the FEAR framework.
    /// Provides the structure for implementing stream-based data collection logic.
    /// Inheritors must implement the logic for processing stream input and producing results.
    /// </summary>
    public abstract class StreamCollectorBase : CollectorBase, IFEARCollector<Stream>
    {
        /// <summary>
        /// The collector type for stream collectors.
        /// </summary>
        protected static CollectorTypeEnum _collectorType = CollectorTypeEnum.Stream;

        /// <summary>
        /// Gets the collector type, which is <see cref="CollectorTypeEnum.Stream"/> for this base class.
        /// </summary>
        public override CollectorTypeEnum CollectorType => _collectorType;

        /// <summary>
        /// Executes the collector using a generic context object, casting it to the appropriate stream context.
        /// </summary>
        /// <param name="context">The context object, expected to be of type <see cref="ICollectorContext{Stream}"/>.</param>
        public override void Execute(object context)
        {
            Execute((ICollectorContext<Stream>)context);
        }

        /// <summary>
        /// Executes the collector logic for a specific stream context.
        /// Must be implemented by derived classes to define stream processing behavior.
        /// </summary>
        /// <param name="context">The stream collector context.</param>
        public abstract void Execute(ICollectorContext<Stream> context);

        /// <summary>
        /// Initializes a new instance of the <see cref="StreamCollectorBase"/> class with the specified resolver.
        /// </summary>
        /// <param name="resolver">The FEAR resolver for component management.</param>
        public StreamCollectorBase(IFEARResolver resolver) : base(resolver)
        {
        }
    }
}
