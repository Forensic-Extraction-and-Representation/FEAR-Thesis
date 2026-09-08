using FEAR.Domain.Agents;
using FEAR.Domain.Model;
using FEAR.Domain.Telemetry;
using FEAR.Runtime.Agents;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace FEAR.Runtime.Execution
{
    public class AgentAdapterRegistry : IAgentAdapterRegistry
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly Lazy<IDictionary<string, Type>> _agentAdapterRegistry;
        protected IDictionary<string, Type> chatInterfaces => _agentAdapterRegistry.Value;

        private readonly Lazy<IEnumerable<AgentDefinition>> _agentDefinitions;
        protected IEnumerable<AgentDefinition> AgentDefinitions => _agentDefinitions.Value;

        protected CaseConfiguration.AgentOptions chatInferenceOptions => executionConfiguration.AgentConfiguration;
        /// <summary>
        /// Service for managing and accessing autopsy case configurations.
        /// </summary>
        private readonly CaseConfiguration executionConfiguration;
        private readonly IFEARTelemetrySignalService _telemetryService;

        public AgentAdapterRegistry(CaseConfiguration config, IFEARTelemetrySignalService telemetryService,
            IServiceProvider serviceProvider, IFEARTelemetryDestinationProvider destinationProvider)
        {
            destinationProvider.RegisterDestination("LogFile", AgentAdapterConstants.TelemetrySource, new Dictionary<string, object> { { "LogFileName", $"fear-queueservice-{DateTime.UtcNow:yyyyMMddHHmm}.log" } });

            executionConfiguration = config;
            _telemetryService = telemetryService;
            _serviceProvider = serviceProvider;
            _agentAdapterRegistry = new Lazy<IDictionary<string, Type>>(() =>
            {
                // Find all classes that have the RegisteredChatInterfaceAttribute
                return AppDomain.CurrentDomain.GetAssemblies()
                    .SelectMany(assembly => assembly.GetTypes())
                    .Where(type => typeof(IAgentAdapter).IsAssignableFrom(type) && !type.IsAbstract)
                    .Where(type => type.GetCustomAttributes(typeof(RegisteredAgentAdapterAttribute), false).Any())
                    .ToDictionary(type => type.GetCustomAttribute<RegisteredAgentAdapterAttribute>().Name, type => type);
            });
            _agentDefinitions = new Lazy<IEnumerable<AgentDefinition>>(() =>
            {
                var agents = new List<AgentDefinition>();
                var configuredAgents = chatInferenceOptions.Agents;
                foreach (var agent in configuredAgents)
                {
                    // Process each section to create AgentIDChainEntry objects.
                    // This could involve deserializing the section into an AgentIDChainEntry object.
                    var agentDef = new AgentDefinition
                    {
                        AgentIdentifier = agent.Key,
                        AgentName = agent.Value.AgentName,
                        AgentType = agent.Value.AgentType,
                        Description = agent.Value.Description,
                        InputChain = agent.Value.InputChain,
                        AgentOptions = agent.Value.AgentOptions
                    };

                    agents.Add(agentDef);
                }

                return agents;
            });
        }

        public IEnumerable<AgentDefinition> GetAgents()
        {
            // Return the list of agent chain entries.
            return AgentDefinitions;
        }

        public IChatAgentAdapter GetChatAgentAdapter(string targetInterface) => GetAgentAdapter(targetInterface, RequestedAgentAdapterType.Chat) as IChatAgentAdapter;

        public IAgentAdapter GetAgentAdapter(string targetInterface, RequestedAgentAdapterType requestedAgentType)
        {
            // Create instances of IChatInterface based on the agent chain entries.
            var matchingAgents = AgentDefinitions.Where(a => a.AgentIdentifier == targetInterface || a.AgentName == targetInterface);
            if (!matchingAgents.Any())
            {
                // Try to get the default agent
                string defaultAgent = chatInferenceOptions.DefaultAgent;
                matchingAgents = AgentDefinitions.Where(a => a.AgentIdentifier == defaultAgent || a.AgentName == defaultAgent);
            }

            if (matchingAgents.Any())
            {
                foreach (var entry in matchingAgents)
                {
                    var agentAdapterInstanceType = _agentAdapterRegistry.Value[entry.AgentType];

                    if (agentAdapterInstanceType == null)
                        continue;

                    if (requestedAgentType != RequestedAgentAdapterType.Any)
                    {
                        if (agentAdapterInstanceType.GetCustomAttribute<RegisteredAgentAdapterAttribute>() is not RegisteredAgentAdapterAttribute attr)
                        {
                            _telemetryService.SendSignal(new GenericFEARTelemetrySignal(AgentAdapterConstants.TelemetrySource, FEARTelemetrySerializationEnum.Raw, FEARTelemetrySignalTypeEnum.Exception)
                                    .WithSignalData($"Agent adapter for agent '{entry.AgentName}' (ID: {entry.AgentIdentifier} ) does not match the requested type '{requestedAgentType}'."));
                            throw new InvalidOperationException($"Agent adapter for agent '{entry.AgentName}' (ID: {entry.AgentIdentifier} ) does not match the requested type '{requestedAgentType}'.");
                        }

                        if (attr.AgentAdapterType.ToString().ToLower() != requestedAgentType.ToString().ToLower())
                            continue;
                    }

                    var agentAdapterInstance = CreateChatInterface(entry);

                    if (agentAdapterInstance != null)
                    {
                        return agentAdapterInstance;
                    }
                    else
                    {
                        _telemetryService.SendSignal(new GenericFEARTelemetrySignal(AgentAdapterConstants.TelemetrySource, FEARTelemetrySerializationEnum.Raw, FEARTelemetrySignalTypeEnum.Exception)
                                .WithSignalData($"Agent adapter for agent '{entry.AgentName}' (ID: {entry.AgentIdentifier} ) not found."));
                        throw new InvalidOperationException($"Agent adapter for agent '{entry.AgentName}' (ID: {entry.AgentIdentifier} ) not found.");
                    }
                }
            }

            _telemetryService.SendSignal(new GenericFEARTelemetrySignal(AgentAdapterConstants.TelemetrySource, FEARTelemetrySerializationEnum.Raw, FEARTelemetrySignalTypeEnum.Exception)
                        .WithSignalData($"Agent adapter for agent '{targetInterface}' not found."));
            throw new InvalidOperationException($"Agent adapter for agent '{targetInterface}' not found.");
        }

        public IAgentAdapter CreateChatInterface(AgentDefinition entry)
        {
            if (chatInterfaces.TryGetValue(entry.AgentType, out Type? chatInterfaceType))
            {
                // Create an instance of the chat interface using reflection.
                return ActivatorUtilities.CreateInstance(_serviceProvider, chatInterfaceType, entry, entry.AgentOptions) as IAgentAdapter
                    ?? (IAgentAdapter)Activator.CreateInstance(chatInterfaceType, entry, entry.AgentOptions);
            }
            else
            {
                throw new InvalidOperationException($"Chat interface for agent '{entry.AgentName}' (ID:{entry.AgentIdentifier}) not found.");
            }
        }
    }
}
