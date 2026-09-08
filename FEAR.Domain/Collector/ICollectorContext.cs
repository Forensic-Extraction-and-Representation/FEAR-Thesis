using FEAR.Domain.Infrastructure;
using FEAR.Domain.Provenance;

namespace FEAR.Domain.Collector
{
    /// <summary>
    /// Context for a collector that provides access to the input data,
    /// provenance information, and the result of the collection process.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface ICollectorContext<T>
    {
        /// <summary>
        /// Provenance information for the input data.
        /// </summary>
        IProvenance InputProvenance { get;  }

        /// <summary>
        /// The input data that the collector is processing.
        /// </summary>
        T AcceptData { get;  }

        /// <summary>
        /// The result of the collection process, which contains the
        /// entity/entities produced by the collector.
        /// </summary>
        IEntityResult Result { get; set; }

        IExecutionServiceProvider ExecutionServiceProvider { get; }
    }
}
