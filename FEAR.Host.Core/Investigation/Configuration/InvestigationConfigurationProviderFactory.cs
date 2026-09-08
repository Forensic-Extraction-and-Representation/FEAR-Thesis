using FEAR.Domain.Configuration;
using Microsoft.Extensions.Configuration;

namespace FEAR.Host.Core.Investigation.Configuration
{
    public class InvestigationConfigurationProviderFactory
    {
        private readonly IConfiguration _config;
        public InvestigationConfigurationProviderFactory(IConfiguration config)
        {
            _config = config;
        }

        public event OnInvestigationConfigurationChangedEventHandler InvestigationConfigurationChanged;

        public IInvestigationConfigurationProvider Create(IServiceProvider serviceProvider)
        {
            string configurationConnection = _config.GetValue<string>("ConnectionStrings:InvestigationConfigurationConnection");
            // The string needs to be split by ; which will result in a set of key -value pairs
            if (string.IsNullOrEmpty(configurationConnection))
            {
                throw new ArgumentException("Configuration Connection is not set in the configuration.");
            }

            Dictionary<string, string> connectionParameters = DelimitedParameterParser.Parse(configurationConnection);
           
            if (connectionParameters.ContainsKey("Provider"))
            {
                Type type = Type.GetType(connectionParameters["Provider"]);
                if (type == null)
                {
                    throw new ArgumentException($"Provider type {connectionParameters["Provider"]} not found.");
                }
                var configProviderInstance = (IInvestigationConfigurationProvider)Activator.CreateInstance(type, connectionParameters);
                configProviderInstance.Initialize(serviceProvider);
                return configProviderInstance;
            }
            else
            {
                throw new ArgumentException("Provider type is not specified in the configuration.");
            }

        }
    }
}
