using FEAR.Domain.Model;
using FEAR.Host.Core.Investigation.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace FEAR.Host.Core.Services
{
    /// <summary>
    /// Provides services for managing and accessing case-specific configuration files for autopsy cases.
    /// Handles creation, retrieval, and existence checks for case configuration files in a specified directory.
    /// </summary>
    public class InvestigationConfigurationService
    {
        private readonly ILogger<InvestigationConfigurationService> _logger;
        private readonly IConfiguration _config;

        /// <summary>
        /// In-memory cache of loaded case configurations, keyed by case name.
        /// </summary>
        protected Dictionary<string, CaseConfiguration> investigationConfigurations = new Dictionary<string, CaseConfiguration>();

        private readonly Lazy<IInvestigationConfigurationProvider> _investigationConfigurationProvider = null;
        private readonly IServiceProvider _serviceProvider;
        private readonly InvestigationConfigurationProviderFactory _invConfigProviderFactory;
        protected IInvestigationConfigurationProvider InvestigationConfigurationProvider => _investigationConfigurationProvider.Value;


        /// <summary>
        /// Initializes a new instance of the <see cref="InvestigationConfigurationService"/> class.
        /// </summary>
        /// <param name="logger">The logger for diagnostic and informational messages.</param>
        /// <param name="config">The application configuration instance.</param>
        public InvestigationConfigurationService(ILogger<InvestigationConfigurationService> logger, InvestigationConfigurationProviderFactory invConfigProviderFactory, IConfiguration config, IServiceProvider serviceProvider)
        {
            _logger = logger;
            _config = config;
            _serviceProvider = serviceProvider;
            _invConfigProviderFactory = invConfigProviderFactory;
            _invConfigProviderFactory.InvestigationConfigurationChanged += InvConfigProvider_OnInvestigationConfigurationChange;
            _investigationConfigurationProvider = new Lazy<IInvestigationConfigurationProvider>(() =>
            {
                return _invConfigProviderFactory.Create(_serviceProvider);
            });
        }

        private void InvConfigProvider_OnInvestigationConfigurationChange(object sender, OnInvestigationConfigurationChangeEventArgs e)
        {
            if (investigationConfigurations.ContainsKey(e.CaseName))
            {
                investigationConfigurations.Remove(e.CaseName);
                _logger.LogInformation($"Investigaiton configuration for {e.CaseName} has been reloaded.");
            }

        }

        public List<string> GetAllInvestigationNames() => InvestigationConfigurationProvider.GetAllInvestigationNames();

        /// <summary>
        /// Creates a new configuration file for the specified case if it does not already exist.
        /// Returns the loaded configuration for the case.
        /// </summary>
        /// <param name="invName">The name of the case.</param>
        /// <returns>The <see cref="IConfiguration"/> instance for the case.</returns>
        public CaseConfiguration CreateInvestigationConfiguration(string invName)
        {
            InvestigationConfigurationProvider.CreateInvestigationConfiguration(invName);

            return GetInvestigationConfiguration(invName);
        }

        /// <summary>
        /// Loads and returns the configuration for the specified case.
        /// If not already loaded, reads from the configuration file and caches the result.
        /// </summary>
        /// <param name="invName">The name of the case.</param>
        /// <returns>The <see cref="IConfiguration"/> instance for the case.</returns>
        /// <exception cref="Exception">Thrown if the configuration file does not exist.</exception>
        public CaseConfiguration GetInvestigationConfiguration(string invName)
        {
            if (!investigationConfigurations.ContainsKey(invName))
            {
                var invConfig = InvestigationConfigurationProvider.GetInvestigationConfiguration(invName);
                if (invConfig == null)
                {
                    throw new Exception($"Configuration for investigation {invName} does not exist.");
                }
                investigationConfigurations[invName] = invConfig;
            }

            return investigationConfigurations[invName];
        }

        public bool SaveInvestigationConfiguration(string invName, CaseConfiguration configuration)
        {
            if(investigationConfigurations.ContainsKey(invName))
            {
                investigationConfigurations[invName] = configuration;
            }
            else
            {
                investigationConfigurations.Add(invName, configuration);
            }

            return InvestigationConfigurationProvider.SaveInvestigationConfiguration(invName, configuration);
        }
    }
}
