using FEAR.Domain.Dto.InvestigationStore;
using FEAR.Domain.Investigation;
using Microsoft.EntityFrameworkCore;

namespace FEAR.Domain.Database
{
    /// <summary>
    /// Provides methods for retrieving and managing investigation information.
    /// </summary>
    public interface IInvestigationManagementDbContext
    {
        /// <summary>
        /// Retrieves a list of all investigations.
        /// </summary>
        /// <returns>A list of <see cref="InvestigationInfo"/> objects.</returns>
        IList<InvestigationInfo> GetInvestigations();

        /// <summary>
        /// Retrieves a specific investigation by its unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the investigation.</param>
        /// <returns>The <see cref="InvestigationInfo"/> if found; otherwise, null.</returns>
        InvestigationInfo GetInvestigation(Guid id);

        /// <summary>
        /// Retrieves a specific investigation by its name.
        /// </summary>
        /// <param name="name">The name of the investigation.</param>
        /// <returns>The <see cref="InvestigationInfo"/> if found; otherwise, null.</returns>
        InvestigationInfo GetInvestigationByName(string? name);

        InvestigationConfiguration GetInvestigationConfiguration(Guid investigationId);

        InvestigationConfiguration GetInvestigationConfigurationByName(string investigationName);
        void SaveInvestigationConfiguration(InvestigationConfiguration invConfig);
    }

    /// <summary>
    /// Generic interface for managing investigations, configurations, and queries in the database context.
    /// </summary>
    /// <typeparam name="TInvestigationInfo">The type representing investigation information.</typeparam>
    /// <typeparam name="TInvestigationConfiguration">The type representing investigation configuration.</typeparam>
    /// <typeparam name="TInvestigationQuery">The type representing investigation queries.</typeparam>
    public interface IInvestigationManagementDbContext<TInvestigationInfo, TInvestigationConfiguration, TInvestigationQuery> 
        : IInvestigationManagementDbContext, IInvestigationManagementStore<TInvestigationInfo, TInvestigationConfiguration, TInvestigationQuery>
        where TInvestigationInfo : InvestigationInfo
        where TInvestigationConfiguration : InvestigationConfiguration
        where TInvestigationQuery: InvestigationQuery
    {
        /// <summary>
        /// Gets or sets the investigations in the database.
        /// </summary>
        DbSet<TInvestigationInfo> Investigations { get; set; }

        /// <summary>
        /// Gets or sets the investigation configurations in the database.
        /// </summary>
        DbSet<TInvestigationConfiguration> InvestigationConfigurations { get; set; }

        /// <summary>
        /// Gets or sets the investigation queries in the database.
        /// </summary>
        DbSet<TInvestigationQuery> InvestigationQuerys { get; set; }

        new TInvestigationConfiguration GetInvestigationConfiguration(Guid investigationId);

        new TInvestigationConfiguration GetInvestigationConfigurationByName(string investigationName);
        new void SaveInvestigationConfiguration(TInvestigationConfiguration invConfig);
    }
}