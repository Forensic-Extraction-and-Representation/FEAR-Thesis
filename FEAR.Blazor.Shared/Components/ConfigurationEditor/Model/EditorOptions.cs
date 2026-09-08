using System.Linq;

namespace FEAR.Blazor.Shared.Components.ConfigurationEditor.Model
{

    public class EditorOptions
    {
        public FormRoot Root { get; set; }
        public string Title { get; set; }
        public Dictionary<string, string> RegexValidationRules { get; set; } = new Dictionary<string, string>();
    }
}