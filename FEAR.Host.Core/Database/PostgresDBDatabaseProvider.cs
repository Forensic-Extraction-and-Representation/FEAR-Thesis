using FEAR.Domain.Configuration;
using Microsoft.EntityFrameworkCore;

namespace FEAR.Host.Core.Database
{
    /// <summary>
    /// Provides configuration for connecting Entity Framework Core to a PostgreSQL database using the Npgsql provider.
    /// Implements <see cref="IDatabaseConfigurationProvider"/> to allow dynamic configuration from application settings.
    /// </summary>
    public class PostgresDBDatabaseProvider : IDatabaseConfigurationProvider
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PostgresDBDatabaseProvider"/> class.
        /// </summary>
        public PostgresDBDatabaseProvider()
        {
        }

        /// <summary>
        /// Configures the <see cref="DbContextOptionsBuilder"/> to use PostgreSQL with the specified options and connection string.
        /// Reads the server version and type from <paramref name="dbConfig"/> and applies them to the provider configuration.
        /// </summary>
        /// <param name="options">The DbContext options builder to configure.</param>
        /// <param name="dbConfig">The database configuration options loaded from configuration files.</param>
        /// <param name="connectionString">The connection string for the PostgreSQL database.</param>
        public void ConstructService(DbContextOptionsBuilder options, DatabaseConfigurationOptions dbConfig, string connectionString)
        {
            options.UseNpgsql(connectionString, b=>b.MigrationsAssembly(dbConfig.MigrationAssembly));
        }
    }
}
