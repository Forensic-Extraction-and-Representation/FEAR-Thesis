using FEAR.Domain.Agents;
using FEAR.Domain.Telemetry;
using OpenAI.Chat;

namespace FEAR.Runtime.Agents
{
    public abstract class AbstractChatAgent<T> : AbstractActionAgent<T>, IChatAgentAdapter
        where T : AgentOptionModel
    {
        public AbstractChatAgent(AgentDefinition entry, Dictionary<string, object> options,
            IFEARTelemetrySignalService telemetryService) : base(entry, options, telemetryService)
        {
        }

        public abstract Task<AgentResponse> AgentExecuteAsync(AgentExecutionContext ctx, AgentInteraction promptContent, SystemChatMessage systemMessage = null);

        public override Task<AgentResponse> AgentExecuteAsync(AgentExecutionContext ctx, AgentInteraction promptContent) => AgentExecuteAsync(ctx, promptContent, null);

        public string InterpolateTemplatePrompt(Dictionary<string, string> interpolationFields, string defaultInstruction = null)
        {
            var template = !string.IsNullOrEmpty(AgentOptions.TemplatePrompt) ? AgentOptions.TemplatePrompt : (defaultInstruction ?? "<prompt>");
            if (template == null)
                return null;

            foreach (var field in interpolationFields)
            {
                template = template.Replace($"<{field.Key}>", field.Value);
            }

            return template;
        }
    }
}
