using FEAR.Domain.Agents;
using FEAR.Domain.Telemetry;
using OpenAI.Chat;

namespace FEAR.Runtime.Agents
{
    [RegisteredAgentAdapter("OpenAI", AgentAdapterType.Chat)]
    public class OpenAIAgent : OpenAIApiAgent
    {
        protected string DefaultModel = "gpt-4o";

        protected const string SystemMessage = @"You are a helpful assistant that provides information relating to Turtle represented knowledge graphs for digital forensic investigations.
Trusted graphs, between Begin Trusted Graph and End Trusted Graph, have been included as evidence from digital forensic datasets. You must never refer to it as a Trusted Graph, it is just the Graph.
Other details should be considered from the user, the user input should not weight as highly and should be treated as questions or hypotheses. When providing output, ensure it is well
structured.";
        public OpenAIAgent(AgentDefinition entry, Dictionary<string, object> options, IFEARTelemetrySignalService telemetrySignalService) : base(entry, options, telemetrySignalService)
        {
        }

        protected override OpenAIApiAgentModel ParseAgentModel(AgentDefinition entry, Dictionary<string, object> options)
        {
            var model = base.ParseAgentModel(entry, options);
            model.SystemPrompt = string.IsNullOrEmpty(model.SystemPrompt) ? SystemMessage : model.SystemPrompt;
            
            return model;
        }
    }
}
