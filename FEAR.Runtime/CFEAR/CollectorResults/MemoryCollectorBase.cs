using FEAR.Domain.Infrastructure;

namespace FEAR.Runtime.CFEAR.CollectorResults
{
    /// <summary>
    /// Base class for collectors that operate on memory sources within the FEAR framework.
    /// Inherits file collector behavior, allowing memory-based collectors to leverage file collection logic.
    /// Intended for extension by collectors that process memory data as part of forensic or analytical workflows.
    /// </summary>
    public abstract class MemoryCollectorBase : FileCollectorBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MemoryCollectorBase"/> class with the specified resolver.
        /// </summary>
        /// <param name="resolver">The FEAR resolver for component management.</param>
        public MemoryCollectorBase(IFEARResolver resolver) : base(resolver)
        {
        }
    }
}
