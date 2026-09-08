using FEAR.Domain.Agents;
using FEAR.Domain.Telemetry;
using OpenAI.Chat;

namespace FEAR.Runtime.Agents
{
    public class DummyAgentOptionModel : AgentOptionModel
    {
    }
    [RegisteredAgentAdapter("Dummy", AgentAdapterType.Chat)]
    public class DummyAIAgent : AbstractChatAgent<DummyAgentOptionModel>
    {
        protected override DummyAgentOptionModel ParseAgentModel(AgentDefinition entry, Dictionary<string, object> options)
        {
            return new DummyAgentOptionModel();
        }

        public DummyAIAgent(AgentDefinition entry, Dictionary<string, object> options, IFEARTelemetrySignalService telemetrySignalService) : base(entry, options, telemetrySignalService)
        {
        }

        public override async Task<AgentResponse> AgentExecuteAsync(AgentExecutionContext ctx, AgentInteraction promptContext, SystemChatMessage systemMessage = null)
        {
            return new AgentResponse() {
                CompletionTimestamp = DateTime.UtcNow,
                Content = $"This is a dummy response from the DummyAIAgent. No real inference was performed. Received {ctx.AgentInteractions.Count} entries, the entries started with \n\n"+
                $"{String.Join("\n", ctx.AgentInteractions.Select(t=>" * "+t.Message.Substring(0,Math.Min(t.Message.Length, 30))).ToArray())}" 
            };
        }
    }
}
