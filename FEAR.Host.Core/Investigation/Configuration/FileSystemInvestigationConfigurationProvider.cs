using FEAR.Domain.Model;
using Microsoft.Extensions.Configuration;
using System.Text.Json;

namespace FEAR.Host.Core.Investigation.Configuration
{
    public class FileSystemInvestigationConfigurationProvider : BaseInvestigationConfigurationProvider
    {
        private readonly string _configurationDirectory;
        public string ConfigurationDirectory => _configurationDirectory;

        public FileSystemInvestigationConfigurationProvider(Dictionary<string, string> configurationOptions)
        {
            _configurationDirectory = configurationOptions["Directory"];
        }

        /// <summary>
        /// Checks if a configuration file exists for the specified case.
        /// </summary>
        /// <param name="caseName">The name of the case.</param>
        /// <returns>True if the configuration file exists; otherwise, false.</returns>
        private bool InvestigationConfigFileExists(string caseName)
        {
            bool caseConfigFileExists = File.Exists($"{ConfigurationDirectory}/{caseName}.json");
            return caseConfigFileExists;
        }

        public override CaseConfiguration CreateInvestigationConfiguration(string caseName)
        {
            if (!InvestigationConfigFileExists(caseName))
            {
                CaseConfiguration hostedCaseConfiguration = new CaseConfiguration(caseName, ConfigurationDirectory);

                // Write the configuration to a JSON file
                var jsonSerializerOptions = new JsonSerializerOptions
                {
                    WriteIndented = true,
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                };
                string text = JsonSerializer.Serialize(hostedCaseConfiguration, jsonSerializerOptions);
                // Ensure the directory exists
                if (!Directory.Exists(ConfigurationDirectory))
                {
                    Directory.CreateDirectory(ConfigurationDirectory);
                }

                File.WriteAllText($"{ConfigurationDirectory}/{caseName}.json", text);
                return hostedCaseConfiguration;
            }

            return null;
        }

        public override List<string> GetAllInvestigationNames()
        {
            if (!Directory.Exists(ConfigurationDirectory))
            {
                return new List<string>();
            }
            // Get all JSON files in the configuration directory
            var files = Directory.GetFiles(ConfigurationDirectory, "*.json");
            // Extract case names from file names (without extension)
            return files.Select(Path.GetFileNameWithoutExtension).ToList();
        }

        public override bool InvestigationConfigurationExists(string caseName) =>
            InvestigationConfigFileExists(caseName);

        public override CaseConfiguration GetInvestigationConfiguration(string caseName)
        {
            if (InvestigationConfigFileExists(caseName))
            {
                var caseConfig = new ConfigurationBuilder()
                    .SetBasePath(ConfigurationDirectory)
                    .AddJsonFile($"{caseName}.json")
                    .Build();

                return caseConfig.Get<CaseConfiguration>();
            }
            else
            {
                throw new Exception($"Case configuration for {caseName} does not exist");
            }
        }

        public override bool SaveInvestigationConfiguration(string caseName, CaseConfiguration configuration)
        {
            if (InvestigationConfigFileExists(caseName))
            {
                var jsonSerializerOptions = new JsonSerializerOptions
                {
                    WriteIndented = true,
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                };
                string text = JsonSerializer.Serialize(configuration, jsonSerializerOptions);
                File.WriteAllText($"{ConfigurationDirectory}/{caseName}.json", text);
                return true;
            }
            else
            {
                throw new Exception($"Case configuration for {caseName} does not exist");
            }
        }
    }
}
