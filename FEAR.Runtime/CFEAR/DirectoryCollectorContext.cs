using FEAR.Domain.Collector;
using FEAR.Domain.Infrastructure;
using FEAR.Domain.Provenance;

namespace FEAR.CFEAR
{
    /// <summary>
    /// Collector context for directory-based collection operations within the FEAR framework.
    /// Inherits from <see cref="FileCollectorContext"/> and provides provenance and context
    /// for collectors that process directories as input.
    /// </summary>
    public class DirectoryCollectorContext : FileCollectorContext
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DirectoryCollectorContext"/> class
        /// with the specified collector and input provenance.
        /// </summary>
        /// <param name="collector">The collector instance associated with this context.</param>
        /// <param name="inputProvenance">The provenance information for the input directory.</param>
        public DirectoryCollectorContext(IFEARCollector collector, IProvenance inputProvenance, IExecutionServiceProvider executionProvider) : base(collector, inputProvenance, executionProvider) { }
    }
}
