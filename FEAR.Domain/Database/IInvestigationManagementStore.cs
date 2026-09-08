using FEAR.Domain.Dto.InvestigationStore;

namespace FEAR.Domain.Investigation
{
    /// <summary>
    /// Defines methods for retrieving investigation information and details from a data store.
    /// </summary>
    /// <typeparam name="TInvestigationInfo">The type representing investigation information.</typeparam>
    /// <typeparam name="TInvestigationConfiguration">The type representing investigation configuration.</typeparam>
    /// <typeparam name="TInvestigationQuery">The type representing investigation (SPARQL) queries.</typeparam>
    public interface IInvestigationManagementStore<TInvestigationInfo, TInvestigationConfiguration, TInvestigationQuery>
        where TInvestigationInfo : InvestigationInfo
        where TInvestigationConfiguration : InvestigationConfiguration
        where TInvestigationQuery : InvestigationQuery
    {
        /// <summary>
        /// Retrieves a list of all investigations from the store.
        /// </summary>
        /// <returns>A list of <typeparamref name="TInvestigationInfo"/> objects.</returns>
        IList<TInvestigationInfo> GetInvestigations();

        /// <summary>
        /// Retrieves a specific investigation by its unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the investigation.</param>
        /// <returns>The <typeparamref name="TInvestigationInfo"/> if found; otherwise, null.</returns>
        TInvestigationInfo GetInvestigation(Guid id);
    }
}
