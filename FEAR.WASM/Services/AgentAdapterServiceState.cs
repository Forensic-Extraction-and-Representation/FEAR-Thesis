using FEAR.Domain.Agents;

namespace FEAR.WASM.Services
{
    public class AgentAdapterServiceState
    {
        public AgentExecutionContext ChatContext { get; set; }

        public Dictionary<string, bool> Options { get; set; } = new Dictionary<string, bool>();
        public Action<bool> IncludeGraphAction { get; set; } = _ => { };
    }
}