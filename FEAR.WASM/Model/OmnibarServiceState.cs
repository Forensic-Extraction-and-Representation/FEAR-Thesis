namespace FEAR.WASM.Model
{
    public class WebUIConfiguration
    {
        public string InvestigationName { get;set;}
    }

    public class OmnibarServiceState
    {
        public string ActiveInput { get; set; } = "query"; // Default to query input
        public bool CanDownload { get; set; } = false;
        public Dictionary<string, Dictionary<string, bool>> ToolbarState = new Dictionary<string, Dictionary<string, bool>>
        {
            { "query", new Dictionary<string, bool> { { "addAsLayer", false } } },
            { "chat", new Dictionary<string, bool> { { "includeGraph", false } } }
        };
    }
}
