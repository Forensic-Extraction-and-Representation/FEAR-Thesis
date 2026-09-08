using Microsoft.Extensions.Configuration;

namespace FEAR.Domain.Agents
{
    public class AgentDefinition
    {
        public string AgentIdentifier { get; set; } = "AG-" + Guid.NewGuid().ToString("N").Substring(0, 10).ToLower();
        public string AgentName { get; set; } = string.Empty;
        public string AgentType { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string[] InputChain { get; set; } = Array.Empty<string>();
        public Dictionary<string,object> AgentOptions { get; set; } = new Dictionary<string, object>();
    }
}
