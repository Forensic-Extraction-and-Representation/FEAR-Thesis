namespace FEAR.Domain.Agents
{
    public class AgentResponse
    {
        public DateTimeOffset CompletionTimestamp { get; set; }
        public string Content { get; set; }
        public string AgentId { get; set; }
    }
}
