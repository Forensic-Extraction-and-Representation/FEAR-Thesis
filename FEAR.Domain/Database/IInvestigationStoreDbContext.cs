using FEAR.Domain.Investigation;

namespace FEAR.Domain.Database
{
    /// <summary>
    /// Interface for the Investigation Store database context.
    /// An Investigation Store is a database that contains metadata about investigations.
    /// </summary>
    public interface IInvestigationStoreDbContext : IInvestigationStore
    {
    }
}