using System.Text.Json.Serialization;

namespace FEAR.Blazor.Shared.Components.ConfigurationEditor.Model
{
    public class FormRoot : FormElement
    {
        [JsonIgnore]
        public override FormElement Parent { get; set; } = null;
        public override string PropertyPath => "";

        // We can only have one entry in the Options list if true
        public bool IsArray { get; set; }
    }
}
