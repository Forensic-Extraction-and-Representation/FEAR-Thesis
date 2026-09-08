using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using FEAR.Domain.Arguments;
using FEAR.Runtime.Execution;

namespace FEAR.CLI.Arguments
{
    public class FearCliOptions : FearExecutionOptions<FearCliArguments>
    {
        public IEnumerable<IFearCliDataSource> FearCliDataSources { get; set; }
        public FearCliOptions(FearCliArguments arguments) : base(arguments, "FEAR-Cli")
        {
            LoadConfigurationFunc = LoadConfiguration;
            PostApplyConfiguration = PostApplyConfigurationInternal;
        }

        private IConfiguration? LoadConfiguration()
        {
            IConfiguration _configuration = null;
            if (File.Exists(TypedArguments.ConfigurationFile.Value))
            {
                _configuration = new ConfigurationBuilder()
                    .AddJsonFile(TypedArguments.ConfigurationFile.Value)
                    .Build();
            }

            return _configuration;
        }

        public HostApplicationBuilder PostApplyConfigurationInternal(HostApplicationBuilder builder, IConfiguration configuration)
        {
            var consoleOutput = Configuration.GetValue<bool?>("ConsoleOutput");
            if (consoleOutput.HasValue)
            {
                TypedArguments.ConsoleOutputOption = new Parameter<bool>(consoleOutput.Value);
            }

            var visualization = Configuration.GetValue<bool?>("Visualize") ?? Configuration.GetValue<bool?>("Visualization")
                ?? Configuration.GetValue<bool?>("Visualisation") ?? Configuration.GetValue<bool?>("Visualise");
            if (visualization.HasValue)
            {
                TypedArguments.VisualizeOption = new Parameter<bool>(visualization.Value);
            }

            var artifactDirectory = Configuration.GetValue<string>("ArtifactsDirectory");
            if (artifactDirectory != null)
            {
                TypedArguments.ArtifactsDirectory = new Parameter<string>() { Value = artifactDirectory };
            }

            var webservice = Configuration.GetValue<string>("WebService");
            if (webservice != null)
            {
                TypedArguments.WebServiceAddressOption = new Parameter<string>() { Value = webservice };
            }

            var statistics = Configuration.GetValue<bool?>("Statistics");
            if (statistics.HasValue)
            {
                TypedArguments.StatisticsOption = new Parameter<bool>(statistics.Value);
            }

            return builder;
        }
    }
}
