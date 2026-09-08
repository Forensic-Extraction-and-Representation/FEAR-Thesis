
using FEAR.Runtime.Execution;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace FEAR.Host.Core.Services
{
    public class FearHostedOptions : FearExecutionOptions<FearHostedArguments>
    {
        protected IConfiguration ApplicationConfiguration { get; }

        public FearHostedOptions(IConfiguration config, IConfiguration applicationConfiguration, string defaultTelemetryServiceSource) 
            : base(new FearHostedArguments(config, applicationConfiguration), defaultTelemetryServiceSource)
        {
            ApplicationConfiguration = applicationConfiguration;
            LoadConfigurationFunc = () => config;
        }
    }
}
