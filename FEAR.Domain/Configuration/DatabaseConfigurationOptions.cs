namespace FEAR.Domain.Configuration
{
    /// <summary>
    /// Defines options for configuring a database connection
    /// that are loaded from a configuration file.
    /// </summary>
    public class DatabaseConfigurationOptions
    {
        public string MigrationAssembly { get; set; }

        /// <summary>
        /// The Type of the IDatabaseConfigurationProvider to instantiate.
        /// </summary>
        public string TypeHandlerName { get; set; }

        /// <summary>
        /// A dictionary of options for the database configuration provider
        /// from the configuration file.
        /// </summary>
        public Dictionary<string, string> Options { get; set; } = new Dictionary<string, string>();

        /// <summary>
        /// Creates an instance of the IDatabaseConfigurationProvider
        /// </summary>
        /// <returns>A instance of the IDatabaseConfigurationProvider as defined by the TypeHandlerName</returns>
        public IDatabaseConfigurationProvider CreateProvider()
        {
            return (IDatabaseConfigurationProvider)Activator.CreateInstance(Type.GetType(TypeHandlerName));
        }
    }
}
