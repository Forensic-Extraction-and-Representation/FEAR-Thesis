using Microsoft.EntityFrameworkCore;

namespace FEAR.Domain.Configuration
{
    /// <summary>
    /// Interface for providing database configuration options.
    /// </summary>
    public interface IDatabaseConfigurationProvider
    {
        /// <summary>
        /// Used to add services to the DbContextOptionsBuilder
        /// for the specific database provider.
        /// </summary>
        /// <param name="options">The DbContextOptionsBuilder to configure.</param>
        /// <param name="dbOptions">The database configuration options. This is loaded from the configuration files and are specific to this type of database provider</param>
        /// <param name="connectionString">The connection string from the configuration file to use for the database.</param>
        void ConstructService(DbContextOptionsBuilder options, DatabaseConfigurationOptions dbOptions, string connectionString);
    }
}
