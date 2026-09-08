using FEAR.Domain.Database;
using Microsoft.EntityFrameworkCore;
using FEAR.Domain.Dto.InvestigationStore;
using FEAR.Domain.Model;
using FEAR.Domain.Dto.ColorMap;

namespace FEAR.Host.Core.Database
{
    /// <summary>
    /// Entity Framework Core database context for managing investigations, configurations, and queries.
    /// Implements the <see cref="IInvestigationManagementDbContext{InvestigationInfo, InvestigationConfiguration, InvestigationQuery}"/> interface
    /// to provide CRUD operations and entity mappings for investigation-related data.
    /// </summary>
    public class InvestigationManagementDbContext 
        : DbContext, IInvestigationManagementDbContext<InvestigationInfo, InvestigationConfiguration, InvestigationQuery>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="InvestigationManagementDbContext"/> class with the specified options.
        /// </summary>
        /// <param name="options">The options to be used by the DbContext.</param>
        public InvestigationManagementDbContext(DbContextOptions<InvestigationManagementDbContext> options) : base(options)
        {
        }

        /// <summary>
        /// Configures the entity mappings, table names, and value generators for the investigation model.
        /// </summary>
        /// <param name="modelBuilder">The builder used to construct the model for the context.</param>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<InvestigationInfo>().ToTable("Investigations");
            modelBuilder.Entity<InvestigationInfo>().Property(u => u.InvestigationId).HasValueGenerator<SequenialGuidGenerator>();

            modelBuilder.Entity<InvestigationConfiguration>().ToTable("InvestigationConfigurations");
            modelBuilder.Entity<InvestigationConfiguration>().Property(u => u.InvestigationConfigurationId).HasValueGenerator<SequenialGuidGenerator>();
            modelBuilder.Entity<InvestigationConfiguration>().Property(u=> u.Configuration).HasConversion(
                v => System.Text.Json.JsonSerializer.Serialize(v, (System.Text.Json.JsonSerializerOptions)null),
                v => System.Text.Json.JsonSerializer.Deserialize<CaseConfiguration>(v, (System.Text.Json.JsonSerializerOptions)null));

            modelBuilder.Entity<InvestigationQuery>().ToTable("InvestigationQueries");
            modelBuilder.Entity<InvestigationQuery>().Property(u => u.InvestigationQueryId).HasValueGenerator<SequenialGuidGenerator>();

            modelBuilder.Entity<ColorMapSet>().ToTable("ColorMapSets");
            modelBuilder.Entity<ColorMapSet>().Property(u => u.ColorMapSetId).HasValueGenerator<SequenialGuidGenerator>();
            modelBuilder.Entity<ColorMapSet>().Property(u => u.ColorMaps).HasConversion(
                v => System.Text.Json.JsonSerializer.Serialize(v, (System.Text.Json.JsonSerializerOptions)null),
                v => System.Text.Json.JsonSerializer.Deserialize<IList<FEAR.Domain.Model.ColorMap.ColorMap>>(v, (System.Text.Json.JsonSerializerOptions)null));

        }

        /// <summary>
        /// Retrieves a list of all investigations from the database.
        /// </summary>
        /// <returns>A list of <see cref="InvestigationInfo"/> objects.</returns>
        public IList<InvestigationInfo> GetInvestigations()
        {
            return Investigations.ToList();
        }

        /// <summary>
        /// Retrieves a specific investigation by its unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the investigation.</param>
        /// <returns>The <see cref="InvestigationInfo"/> if found; otherwise, null.</returns>
        public InvestigationInfo GetInvestigation(Guid id)
        {
            return Investigations.FirstOrDefault(i => i.InvestigationId == id);
        }

        /// <summary>
        /// Retrieves a specific investigation by its name.
        /// </summary>
        /// <param name="name">The name of the investigation.</param>
        /// <returns>The <see cref="InvestigationInfo"/> if found; otherwise, null.</returns>
        public InvestigationInfo GetInvestigationByName(string name)
        {
            return Investigations.FirstOrDefault(i => i.Name == name);
        }

        public InvestigationConfiguration GetInvestigationConfiguration(Guid investigationId)
        {
            return InvestigationConfigurations.FirstOrDefault(c => c.InvestigationId == investigationId);
        }

        public InvestigationConfiguration GetInvestigationConfigurationByName(string investigationName)
        {
            var inv = GetInvestigationByName(investigationName);
            if (inv != null)
                return GetInvestigationConfiguration(inv.InvestigationId);
            else
                return null;
        }

        public void SaveInvestigationConfiguration(InvestigationConfiguration invConfig)
        {
            var existingConfig = InvestigationConfigurations.FirstOrDefault(c => c.InvestigationConfigurationId == invConfig.InvestigationConfigurationId);
            if (existingConfig != null)
            {
                Entry(existingConfig).CurrentValues.SetValues(invConfig);
            }
            else
            {
                InvestigationConfigurations.Add(invConfig);
            }

            SaveChanges();
        }

        public DbSet<ColorMapSet> ColorMaps { get; set; }

        /// <summary>
        /// Gets or sets the investigations in the database.
        /// </summary>
        public DbSet<InvestigationInfo> Investigations { get; set; }

        /// <summary>
        /// Gets or sets the investigation configurations in the database.
        /// </summary>
        public DbSet<InvestigationConfiguration> InvestigationConfigurations { get; set; }
        
        /// <summary>
        /// Gets or sets the investigation queries in the database.
        /// </summary>
        public DbSet<InvestigationQuery> InvestigationQuerys { get; set; }
    }
}
