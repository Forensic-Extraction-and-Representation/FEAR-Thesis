using FEAR.Domain.Collector;
using FEAR.Domain.Infrastructure;
using FEAR.Domain.Provenance;

namespace FEAR.CFEAR
{
    /// <summary>
    /// Provides a base implementation for collector contexts in the FEAR framework.
    /// Encapsulates the input provenance, accepted data, and result for a collection operation.
    /// Used by collectors to track the context and results of data collection, including provenance information.
    /// </summary>
    /// <typeparam name="T">The type of data accepted by the collector.</typeparam>
    public abstract class BaseCollectorContext<T> : ICollectorContext<T>
    {
        /// <summary>
        /// Gets the provenance information for the input data being collected.
        /// </summary>
        public IProvenance InputProvenance { get; }

        /// <summary>
        /// Gets or sets the input data that the collector is processing.
        /// </summary>
        public T AcceptData { get; set; }

        /// <summary>
        /// Gets or sets the result of the collection process, containing the produced entity/entities.
        /// </summary>
        public IEntityResult Result { get; set; }

        /// <summary>
        /// Gets or sets the collector instance associated with this context.
        /// </summary>
        protected IFEARCollector Collector { get; set; }

        public IExecutionServiceProvider ExecutionServiceProvider { get; protected set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="BaseCollectorContext{T}"/> class.
        /// </summary>
        /// <param name="collector">The collector instance associated with this context.</param>
        /// <param name="inputProvenance">The provenance information for the input data.</param>
        public BaseCollectorContext(IFEARCollector collector, IProvenance inputProvenance, IExecutionServiceProvider execServiceProvider)
        {
            Collector = collector;
            InputProvenance = inputProvenance;
            ExecutionServiceProvider = execServiceProvider;
        }
    }
}
