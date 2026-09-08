using FEAR.Domain.Dto.InvestigationStore;
using Microsoft.EntityFrameworkCore;

namespace FEAR.Domain.Investigation
{
    /// <summary>
    /// Provides access to the metadata for investigations.
    /// </summary>
    public interface IInvestigationStore
    {
        /// <summary>
        /// Gets the set of investigation metadata entries 
        /// which includes configuration details.
        /// </summary>
        DbSet<InvestigationStoreMeta> Meta { get; }
    }
}
