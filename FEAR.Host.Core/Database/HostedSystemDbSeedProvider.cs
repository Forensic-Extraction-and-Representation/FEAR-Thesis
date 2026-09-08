using FEAR.Domain.Dto.Authentication;
using FEAR.Domain.Helpers;
using FEAR.Host.Core.Identity;
using FEAR.Host.Core.IdentitySeed;
using FEAR.Host.Core.ProtectedEncryptionKey;
using FEAR.Hosted.Domain.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace FEAR.Host.Core.Database
{
    /// <summary>
    /// Provides functionality to seed the hosted system database with initial data.
    /// </summary>
    public class HostedSystemDbSeedProvider
    {
        private IConfiguration _configuration;
        private IHostedSystemDbContext _context;

        /// <summary>
        /// Initializes a new instance of the <see cref="HostedSystemDbSeedProvider"/> class.
        /// </summary>
        /// <param name="configuration">Application configuration for seed options and encryption key names.</param>
        public HostedSystemDbSeedProvider(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        /// <summary>
        /// Seeds the hosted system database with initial data.
        /// Optionally deletes and recreates the database if allowed and configured.
        /// </summary>
        /// <param name="hostedSystemDbContext">The hosted system database context to seed.</param>
        /// <param name="allowDelete">If true, allows deletion and recreation of the database.</param>
        public void Seed(HostedSystemDbContext hostedSystemDbContext, bool allowDelete)
        {
            // Optionally delete the database if configured to reload seed data
            if (_configuration["DatabaseConfigurationOptions:ReloadSeedData"].ToLower() == "true")
            {
                if (allowDelete)
                    hostedSystemDbContext.Database.EnsureDeleted();
            }

            hostedSystemDbContext.Database.Migrate();
            _context = hostedSystemDbContext;
            SeedData();
        }

        /// <summary>
        /// Seeds the database with initial data.
        /// </summary>
        private void SeedData()
        {
        }
    }
}
