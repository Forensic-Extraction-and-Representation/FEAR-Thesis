using FEAR.Domain.Agents;
using FEAR.Domain.Telemetry;

namespace FEAR.Runtime.Agents
{
    public abstract class AbstractActionAgent<T> : IAgentAdapter
        where T : AgentOptionModel
    {
        protected readonly Dictionary<string, object> _options;
        protected readonly IFEARTelemetrySignalService _telemetryService;

        public string AgentName => AgentEntry.AgentName;
        public string AgentId => AgentEntry.AgentName;
        protected T AgentOptions { get; }

        protected AgentDefinition AgentEntry { get; }
        public AgentDefinition GetAgentOptions() => AgentEntry;

        public AbstractActionAgent(AgentDefinition entry, Dictionary<string, object> options,
            IFEARTelemetrySignalService telemetryService)
        {
            AgentEntry = entry ?? throw new ArgumentNullException(nameof(entry), "AgentChainEntry cannot be null.");
            _options = options ?? throw new ArgumentNullException(nameof(options), "Options cannot be null.");

            _telemetryService = telemetryService ?? throw new ArgumentNullException(nameof(telemetryService), "TelemetryService cannot be null.");
            AgentOptions = ParseAgentModel(entry, options);
        }

        protected string GetValueOrDefault(Dictionary<string, object> options, string key, string defaultValue = "", bool throwIfMissing = false)
        {
            if (options.ContainsKey(key) && !string.IsNullOrEmpty(options[key]?.ToString()))
            {
                return options[key].ToString();
            }
            if (throwIfMissing)
            {
                throw new ArgumentNullException(key, $"{key} must be provided in the configuration.");
            }
            return defaultValue;
        }

        public abstract Task<AgentResponse> AgentExecuteAsync(AgentExecutionContext ctx, AgentInteraction promptContent);
        protected abstract T ParseAgentModel(AgentDefinition entry, Dictionary<string, object> options);
    }
}
