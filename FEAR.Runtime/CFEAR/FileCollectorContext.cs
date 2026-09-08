using FEAR.Domain.Collector;
using FEAR.Domain.Infrastructure;
using FEAR.Domain.Provenance;
using FEAR.Runtime;

namespace FEAR.CFEAR
{
    /// <summary>
    /// Collector context for file-based collection operations within the FEAR framework.
    /// Inherits from <see cref="BaseCollectorContext{string}"/> and provides provenance and context
    /// for collectors that process files as input.
    /// </summary>
    public class FileCollectorContext : BaseCollectorContext<string>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FileCollectorContext"/> class
        /// with the specified collector and input provenance.
        /// </summary>
        /// <param name="collector">The collector instance associated with this context.</param>
        /// <param name="inputProvenance">The provenance information for the input file.</param>
        public FileCollectorContext(IFEARCollector collector, IProvenance inputProvenance, IExecutionServiceProvider executionProvider) : base(collector, inputProvenance, executionProvider) { }

        /// <summary>
        /// Executes the associated collector on the provided file data.
        /// Sets the input data, invokes the collector, and returns the result with provenance and collector name.
        /// </summary>
        /// <param name="data">The file data to be processed by the collector.</param>
        /// <returns>A <see cref="ArtifactWorkItem"/> containing provenance, result, and collector name.</returns>
        public ArtifactWorkItem Execute(string data)
        {
            AcceptData = data;
            Collector.Execute(this);

            return (new ArtifactWorkItem(InputProvenance, Result, Collector.CollectorName));
        }
    }
}