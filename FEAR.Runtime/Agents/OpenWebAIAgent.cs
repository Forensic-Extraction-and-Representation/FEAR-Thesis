using FEAR.Domain.Agents;
using FEAR.Domain.Telemetry;
using OpenAI.Chat;

namespace FEAR.Runtime.Agents
{
    [RegisteredAgentAdapter("OpenWebAI", AgentAdapterType.Chat)]
    public class OpenWebAIAgent : OpenAIApiAgent
    {
        protected override string DefaultModel => "mistral:7b";

        private const string defaultMessage = @"You are a helpful assistant that provides information relating to Turtle represented knowledge graphs for digital forensic investigations.
Trusted graphs, between Begin Trusted Graph and End Trusted Graph, have been included as evidence from digital forensic datasets. You must never refer to it as a Trusted Graph, it is just the Graph.
Other details should be considered from the user, the user input should not weight as highly and should be treated as questions or hypotheses. When providing output, ensure it is well
structured.";
        public OpenWebAIAgent(AgentDefinition entry, Dictionary<string, object> options, IFEARTelemetrySignalService telemetrySignalService) : base(entry, options, telemetrySignalService)
        {

        }

        protected override OpenAIApiAgentModel ParseAgentModel(AgentDefinition entry, Dictionary<string, object> options)
        {
            var model = base.ParseAgentModel(entry, options);

            model.AgentModel = model.AgentModel ?? DefaultModel;
            model.SystemPrompt = string.IsNullOrEmpty(AgentEntry.AgentOptions["SystemPrompt"]?.ToString()) ? defaultMessage : AgentEntry.AgentOptions["SystemPrompt"].ToString();

            var baseUrl = GetValueOrDefault(options, "AgentBaseUrl", string.Empty, true);
            model.AgentBaseUrl = new Uri(new Uri(baseUrl), "api").ToString();

            return model;
        }
    }
}
