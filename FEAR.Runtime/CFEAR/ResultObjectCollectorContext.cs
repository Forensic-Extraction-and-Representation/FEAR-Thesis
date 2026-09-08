using FEAR.Domain.Collector;
using FEAR.Domain.KnowledgeGraph.GraphCodify;
using FEAR.Domain.Provenance;
using FEAR.Runtime;
using FEAR.Domain.Infrastructure;

namespace FEAR.CFEAR
{
    /// <summary>
    /// Collector context for result object-based collection operations within the FEAR framework.
    /// Inherits from <see cref="BaseCollectorContext{CollectorResult}"/> and provides provenance and context
    /// for collectors that process <see cref="ArtifactWorkItem"/> objects as input.
    /// Exposes the entity data as a <see cref="CodifierExpandoObject"/> for flexible property access.
    /// </summary>
    public class ResultObjectCollectorContext : BaseCollectorContext<ArtifactWorkItem>
    {
        /// <summary>
        /// The dynamic entity data extracted from the <see cref="ArtifactWorkItem"/>.
        /// </summary>
        public CodifierExpandoObject EntityData { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ResultObjectCollectorContext"/> class
        /// with the specified collector and input provenance.
        /// </summary>
        /// <param name="collector">The collector instance associated with this context.</param>
        /// <param name="inputProvenance">The provenance information for the input result object.</param>
        public ResultObjectCollectorContext(IFEARCollector collector, IProvenance inputProvenance, IExecutionServiceProvider executionService) : base(collector, inputProvenance, executionService) { }

        /// <summary>
        /// Executes the associated collector on the provided <see cref="ArtifactWorkItem"/> data.
        /// Sets the input data, extracts the entity data, invokes the collector, and returns the result with provenance and collector name.
        /// </summary>
        /// <param name="data">The <see cref="ArtifactWorkItem"/> to be processed by the collector.</param>
        /// <returns>A <see cref="ArtifactWorkItem"/> containing provenance, result, and collector name.</returns>
        public ArtifactWorkItem Execute(ArtifactWorkItem data)
        {
            AcceptData = data;
            EntityData = ((EntityResult)data.Entity).Entity;
            Collector.Execute(this);

            return (new ArtifactWorkItem(InputProvenance, Result, Collector.CollectorName));
        }
    }
}
