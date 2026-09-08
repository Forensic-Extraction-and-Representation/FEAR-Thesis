using OpenAI.Chat;

namespace FEAR.Domain.Agents
{
    public enum AgentAdapterType {
        Chat,
        Action,
    }

    public enum RequestedAgentAdapterType
    {
        Chat,
        Action,
        Any,
    }

    /// <summary>
    /// IAgentAdapter defines the contract for agents that can process agent tasks.
    /// </summary>
    public interface IAgentAdapter
    {
        string AgentId { get; }
        string AgentName { get; }
        AgentDefinition GetAgentOptions();
        Task<AgentResponse> AgentExecuteAsync(AgentExecutionContext ctx, AgentInteraction promptContext);
    }

    public interface IChatAgentAdapter : IAgentAdapter
    {
        Task<AgentResponse> AgentExecuteAsync(AgentExecutionContext ctx, AgentInteraction promptContent, SystemChatMessage systemMessage = null);
        string InterpolateTemplatePrompt(Dictionary<string, string> interpolationFields, string defaultInstruction = null);
    }
}
