namespace FEAR.Runtime.Agents
{
    public class OpenAIApiAgentModel : AgentOptionModel
    {
        public string AgentModel { get; set; }
        public string ApiKey { get; set; } = string.Empty;
        public string AgentBaseUrl { get; set; }
    }
}
