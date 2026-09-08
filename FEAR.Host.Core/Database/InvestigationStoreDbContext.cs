using FEAR.Domain.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using FEAR.Domain.Provenance;
using Microsoft.Extensions.DependencyInjection.Extensions;
using FEAR.Domain.Database;
using FEAR.Domain.Configuration;
using FEAR.Domain.Evidence;
using FEAR.Domain.Extensions;
using FEAR.Domain.Dto.InvestigationStore;

namespace FEAR.Host.Core.Database
{
    /// <summary>
    /// Entity Framework Core database context for managing investigation store data, including evidence sources, provenance, and metadata.
    /// Provides configuration for dependency injection and table mappings for investigation-related entities.
    /// </summary>
    public class InvestigationStoreDbContext : DbContext, IInvestigationStoreDbContext
    {
        /// <summary>
        /// Builder for registering <see cref="InvestigationStoreDbContext"/> and related services in the DI container.
        /// </summary>
        public class Builder : IBuilderForContext
        {
            /// <summary>
            /// Configures the DI container to use <see cref="InvestigationStoreDbContext"/> and related stores.
            /// </summary>
            /// <param name="_contextBuilder">The application builder context.</param>
            public void Build(HostApplicationBuilder _contextBuilder)
            {
                if (!_contextBuilder.Services.DoesServiceExist<InvestigationStoreDbContext>())
                {
                    _contextBuilder.Services.AddDbContextPool<InvestigationStoreDbContext>(options =>
                    {
                        DatabaseConfigurationOptions dbOptions = _contextBuilder.Configuration.GetSection("DatabaseConfigurationOptions").Get<DatabaseConfigurationOptions>();
                        var provider = dbOptions.CreateProvider();
                        provider.ConstructService(options, dbOptions, _contextBuilder.Configuration.GetConnectionString("DefaultConnection"));
                    });

                    _contextBuilder.Services.TryAddScoped<IProvenanceStore, DatabaseProvenanceStore>();
                    _contextBuilder.Services.TryAddScoped<IEvidenceStore, DatabaseEvidenceStore>();
                }
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="InvestigationStoreDbContext"/> class with the specified options.
        /// </summary>
        /// <param name="options">The options to be used by the DbContext.</param>
        public InvestigationStoreDbContext(DbContextOptions<InvestigationStoreDbContext> options) : base(options)
        {
        }

        /// <summary>
        /// Configures the entity mappings, table names, and value generators for the investigation store model.
        /// </summary>
        /// <param name="modelBuilder">The builder used to construct the model for the context.</param>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<InvestigationStoreMeta>().ToTable("InvestigationStoreMeta");
            modelBuilder.Entity<InvestigationStoreMeta>().HasKey(u => u.InvestigationId);
            modelBuilder.Entity<ProvenanceEntry>().Property(u => u.ProvenanceId).HasValueGenerator<SequenialGuidGenerator>();

            modelBuilder.Entity<ProvenanceEntry>().ToTable("ProvenanceEntries");
            modelBuilder.Entity<ProvenanceEntry>().HasKey(u => u.ProvenanceId);
            modelBuilder.Entity<ProvenanceEntry>().Property(u => u.ProvenanceId).HasValueGenerator<SequenialGuidGenerator>();
            modelBuilder.Entity<ProvenanceEntry>().Property(u => u.Path).HasDefaultValue("");
        }

        /// <summary>
        /// Gets or sets the evidence sources in the investigation store.
        /// </summary>
        public DbSet<EvidenceSourceEntry> EvidenceSources { get; set; }

        /// <summary>
        /// Gets or sets the provenance entries in the investigation store.
        /// </summary>
        public DbSet<ProvenanceEntry> ProvenanceEntries { get; set; }

        /// <summary>
        /// Gets or sets the metadata for investigations in the store.
        /// </summary>
        public DbSet<InvestigationStoreMeta> Meta { get; set; }
    }
}
