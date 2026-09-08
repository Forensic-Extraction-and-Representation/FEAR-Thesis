namespace FEAR.Domain.Agents
{
    public interface IAgentAdapterRegistry
    {
        IEnumerable<AgentDefinition> GetAgents();
        IChatAgentAdapter GetChatAgentAdapter(string targetInterface);
        IAgentAdapter GetAgentAdapter(string targetInterface, RequestedAgentAdapterType type);
    }
}