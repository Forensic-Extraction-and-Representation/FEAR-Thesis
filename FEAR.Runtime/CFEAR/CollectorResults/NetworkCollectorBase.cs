using FEAR.Domain.Infrastructure;

namespace FEAR.Runtime.CFEAR.CollectorResults
{
    /// <summary>
    /// Base class for collectors that operate on network sources within the FEAR framework.
    /// Inherits file collector behavior, allowing network-based collectors to leverage file collection logic.
    /// Intended for extension by collectors that process data from network locations (e.g., remote shares, URLs, network streams).
    /// </summary>
    public abstract class NetworkCollectorBase : FileCollectorBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NetworkCollectorBase"/> class with the specified resolver.
        /// </summary>
        /// <param name="resolver">The FEAR resolver for component management.</param>
        public NetworkCollectorBase(IFEARResolver resolver) : base(resolver)
        {
        }
    }
}
