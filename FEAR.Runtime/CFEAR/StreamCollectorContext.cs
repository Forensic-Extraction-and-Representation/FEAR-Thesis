using FEAR.Domain.Collector;
using FEAR.Domain.Provenance;
using FEAR.Runtime;
using FEAR.Domain.Infrastructure;

namespace FEAR.CFEAR
{
    /// <summary>
    /// Collector context for stream-based collection operations within the FEAR framework.
    /// Inherits from <see cref="BaseCollectorContext{Stream}"/> and provides provenance and context
    /// for collectors that process streams as input.
    /// </summary>
    public class StreamCollectorContext : BaseCollectorContext<Stream>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="StreamCollectorContext"/> class
        /// with the specified collector and input provenance.
        /// </summary>
        /// <param name="collector">The collector instance associated with this context.</param>
        /// <param name="inputProvenance">The provenance information for the input stream.</param>
        public StreamCollectorContext(IFEARCollector collector, IProvenance inputProvenance, IExecutionServiceProvider executionProvider) : base(collector, inputProvenance, executionProvider) { }

        /// <summary>
        /// Executes the associated collector on the provided stream data.
        /// Sets the input data, invokes the collector, and returns the result with provenance and collector name.
        /// </summary>
        /// <param name="data">The stream to be processed by the collector.</param>
        /// <returns>A <see cref="ArtifactWorkItem"/> containing provenance, result, and collector name.</returns>
        public ArtifactWorkItem Execute(Stream data)
        {
            AcceptData = data;
            Collector.Execute(this);
            
            return (new ArtifactWorkItem(InputProvenance, Result, Collector.CollectorName));
        }

        /// <summary>
        /// Executes the associated collector on the provided byte array by wrapping it in a <see cref="MemoryStream"/>.
        /// </summary>
        /// <param name="data">The byte array to be processed by the collector.</param>
        /// <returns>A <see cref="ArtifactWorkItem"/> containing provenance, result, and collector name.</returns>
        public ArtifactWorkItem Execute(byte[] data) { return Execute(new MemoryStream(data)); }
    }
}
