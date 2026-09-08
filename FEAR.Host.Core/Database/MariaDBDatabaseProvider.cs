using FEAR.Domain.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;

namespace FEAR.Host.Core.Database
{
    /// <summary>
    /// Provides configuration for connecting Entity Framework Core to a MariaDB database using the Pomelo MySQL provider.
    /// Implements <see cref="IDatabaseConfigurationProvider"/> to allow dynamic configuration from application settings.
    /// </summary>
    public class MariaDBDatabaseProvider : IDatabaseConfigurationProvider
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MariaDBDatabaseProvider"/> class.
        /// </summary>
        public MariaDBDatabaseProvider()
        {
        }

        /// <summary>
        /// Configures the <see cref="DbContextOptionsBuilder"/> to use MariaDB with the specified options and connection string.
        /// Reads the server version and type from <paramref name="dbConfig"/> and applies them to the provider configuration.
        /// </summary>
        /// <param name="options">The DbContext options builder to configure.</param>
        /// <param name="dbConfig">The database configuration options loaded from configuration files.</param>
        /// <param name="connectionString">The connection string for the MariaDB database.</param>
        public void ConstructService(DbContextOptionsBuilder options, DatabaseConfigurationOptions dbConfig, string connectionString)
        {
            string verOpt = dbConfig.Options["Version"];
            string stOpt = dbConfig.Options["ServerType"];
            ServerType serverType = (ServerType)Enum.Parse(typeof(ServerType), stOpt);
            
            options.UseMySql(connectionString, ServerVersion.Parse(verOpt, serverType), b=>b.MigrationsAssembly(dbConfig.MigrationAssembly));
        }
    }
}
