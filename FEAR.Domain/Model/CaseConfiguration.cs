using FEAR.Domain.Agents;
using Microsoft.Extensions.Configuration;
using System.ComponentModel.DataAnnotations;
using System.Net.Quic;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Text.Json;

namespace FEAR.Domain.Model
{
    /// <summary>
    /// Represents the configuration for a case, including directory paths, namespace options, and graph management settings.
    /// Used to initialize and manage case-specific settings in the FEAR platform.
    /// </summary>
    public class CaseConfiguration
    {
        /// <summary>
        /// Gets or sets the name of the case.
        /// </summary>
        public string InvestigationName { get; set; }

        public string OntologyOutputFormat { get; set; } = "ttl";

        public string? TranspiledSourceOutputRoot { get; set; } = "";
        public bool? OutputTranspiledSource { get; set; } = false;

        /// <summary>
        /// Gets or sets the directory for precompiled resources for the case.
        /// </summary>
        public string PreCompiledDirectoryOption { get; set; }

        public string ScriptDirectoryOption { get; set; }

        /// <summary>
        /// Local directory where packaged libraries are extract to before being moved into relevant script directories for compilation and execution.
        /// </summary>
        public string PackageDirectoryOption { get; set; }

        /// <summary>
        /// Gets or sets the namespace URI for the case.
        /// </summary>
        public string NamespaceOption { get; set; }

        /// <summary>
        /// Gets or sets the abbreviated namespace for the case.
        /// </summary>
        public string NamespaceAbbrevOption { get; set; }

        public string WorkingDirectory { get; set; }
        /// <summary>
        /// Gets or sets the graph manager options for the case.
        /// </summary>
        public GraphManagerOptions GraphManager { get; set; } = new GraphManagerOptions();

        public AgentOptions? AgentConfiguration { get; set; } = new AgentOptions();

        public ConnectionOptions Connections { get; set; } = new ConnectionOptions();

        public Dictionary<string, string[]> Repositories { get; set; } = new Dictionary<string, string[]>();

        public Dictionary<string, string[]> PrecompiledLibraries { get; set; } = new Dictionary<string, string[]>();
        public List<string> PackagedSources { get; set; } = new List<string>();

        public void TranslateDictionaryObjects()
        {
            // This method can be used to translate any dictionary objects that may have been deserialized as JsonElement into their proper types.
            // For example, if GraphDBConnection was deserialized as Dictionary<string, JsonElement>, we can convert it to Dictionary<string, object>.
            var translatedGraphDBConnection = new Dictionary<string, object>();
            foreach (var kvp in Connections.GraphDBConnection)
            {
                if (kvp.Value is JsonElement jsonElement)
                {
                    translatedGraphDBConnection[kvp.Key] = ConvertJsonElement(jsonElement);
                }
                else
                {
                    translatedGraphDBConnection[kvp.Key] = kvp.Value;
                }
            }
            Connections.GraphDBConnection = translatedGraphDBConnection;

            if (AgentConfiguration != null)
            {
                foreach (var agent in AgentConfiguration.Agents)
                {
                    var translatedOptions = new Dictionary<string, object>();
                    foreach (var optionKvp in agent.Value.AgentOptions)
                        {
                        if (optionKvp.Value is JsonElement jsonElement)
                        {
                            translatedOptions[optionKvp.Key] = ConvertJsonElement(jsonElement);
                        }
                        else
                        {
                            translatedOptions[optionKvp.Key] = optionKvp.Value;
                        }
                    }

                    AgentConfiguration.Agents[agent.Key].AgentOptions = translatedOptions;
                }
            }
        }

        public object ConvertJsonElement(JsonElement jsonElement)
        {
            switch (jsonElement.ValueKind)
            {
                case JsonValueKind.Object:
                    var dict = new Dictionary<string, object>();
                    foreach (var prop in jsonElement.EnumerateObject())
                    {
                        dict[prop.Name] = ConvertJsonElement(prop.Value);
                    }
                    return dict;
                case JsonValueKind.Array:
                    var list = new List<object>();
                    foreach (var item in jsonElement.EnumerateArray())
                    {
                        list.Add(ConvertJsonElement(item));
                    }
                    return list;
                case JsonValueKind.String:
                    return jsonElement.GetString();
                case JsonValueKind.Number:
                    if (jsonElement.TryGetInt64(out long l))
                        return l;
                    else if (jsonElement.TryGetDouble(out double d))
                        return d;
                    else
                        return jsonElement.GetDecimal();
                case JsonValueKind.True:
                case JsonValueKind.False:
                    return jsonElement.GetBoolean();
                case JsonValueKind.Null:
                default:
                    return null;
            }
        }

        /// <summary>
        /// Gets or sets the options for the entity search strategy factory.
        /// </summary>
        public FindEntityStrategyFactoryOptions? FindEntityStrategyFactory { get; set; } = new FindEntityStrategyFactoryOptions();

        /// <summary>
        /// Initializes a new instance of the <see cref="CaseConfiguration"/> class with the specified case name.
        /// Sets default values for directories, namespaces, and options based on the case name.
        /// </summary>
        /// <param name="caseName">The name of the case.</param>
        public CaseConfiguration(string caseName, string configurationDirectory)
        {
            InvestigationName = caseName;
            WorkingDirectory = Path.Combine(configurationDirectory, caseName);
            PreCompiledDirectoryOption = Path.Combine(configurationDirectory, caseName, "PreCompiled");
            PackageDirectoryOption = Path.Combine(configurationDirectory, "Packages");
            NamespaceOption = $"http://{caseName}.fear.graph/";
        }

        public CaseConfiguration()
        {

        }

        /// <summary>
        /// Options for configuring the graph manager.
        /// </summary>
        public class GraphManagerOptions
    {
        /// <summary>
        /// Gets or sets a value indicating whether the graph is remote.
        /// </summary>
        public bool RemoteGraph { get; set; }

        public string? RemoteGraphSetupProvider { get; set; }
    }

    public class AgentOptions
    {
        public bool Enabled { get; set; } = false;
        public string? DefaultAgent { get; set; } = "";
        public Dictionary<string, AgentDefinition> Agents { get; set; } = new Dictionary<string, AgentDefinition>();
    }

    public class ConnectionOptions
    {
        public Dictionary<string, object> GraphDBConnection { get; set; } = new Dictionary<string, object>();

        public Dictionary<string, string>? DatabaseConnections { get; set; } = new Dictionary<string, string>();
    }

    /// <summary>
    /// Options for configuring the entity search strategy factory.
    /// </summary>
    public class FindEntityStrategyFactoryOptions
    {
        /// <summary>
        /// Gets or sets the type name of the entity search strategy factory.
        /// </summary>
        public string? Local { get; set; }

        public string? Remote { get; set; }
        }

        public IConfiguration ToConfiguration()
        {
            var jsonString = JsonSerializer.Serialize(this);
            var config = new ConfigurationBuilder()
                .AddJsonStream(new MemoryStream(System.Text.Encoding.UTF8.GetBytes(jsonString)))
                .Build();

            return config;
        }

        public object GetSection(string v)
        {
            throw new NotImplementedException();
        }

    }
}
