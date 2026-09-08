namespace FEAR.Domain.Investigation
{
    /// <summary>
    /// Defines a contract for managing and retrieving investigation records.
    /// Provides methods to list all investigations and retrieve details for a specific investigation.
    /// </summary>
    public interface IInvestigationManagementStore
    {
        /// <summary>
        /// Retrieves a list of all investigations with summary information.
        /// </summary>
        /// <returns>A list of <see cref="Dto.InvestigationStore.InvestigationInfo"/> objects representing investigations.</returns>
        IList<Dto.InvestigationStore.InvestigationInfo> GetInvestigations();

        /// <summary>
        /// Retrieves detailed information for a specific investigation by its unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the investigation.</param>
        /// <returns>An <see cref="Dto.InvestigationStore.InvestigationInfo"/> object with investigation details.</returns>
        Dto.InvestigationStore.InvestigationInfo RetrieveInvestigation(Guid id);
    }
}
