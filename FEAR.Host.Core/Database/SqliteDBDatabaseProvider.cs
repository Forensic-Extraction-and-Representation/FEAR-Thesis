using FEAR.Domain.Configuration;
using Microsoft.EntityFrameworkCore;

namespace FEAR.Host.Core.Database
{
    /// <summary>
    /// Provides configuration for connecting Entity Framework Core to a SQLite database.
    /// Implements <see cref="IDatabaseConfigurationProvider"/> to allow dynamic configuration from application settings.
    /// </summary>
    public class SqliteDBDatabaseProvider : IDatabaseConfigurationProvider
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SqliteDBDatabaseProvider"/> class.
        /// </summary>
        public SqliteDBDatabaseProvider()
        {
        }

        /// <summary>
        /// Configures the <see cref="DbContextOptionsBuilder"/> to use SQLite with the specified connection string.
        /// </summary>
        /// <param name="options">The DbContextOptionsBuilder to configure.</param>
        /// <param name="dbConfig">The database configuration options loaded from configuration files (not used for SQLite).</param>
        /// <param name="connectionString">The connection string for the SQLite database.</param>
        public void ConstructService(DbContextOptionsBuilder options, DatabaseConfigurationOptions dbConfig, string connectionString)
        {
            options.UseSqlite(connectionString, b => b.MigrationsAssembly(dbConfig.MigrationAssembly));
        }
    }
}
