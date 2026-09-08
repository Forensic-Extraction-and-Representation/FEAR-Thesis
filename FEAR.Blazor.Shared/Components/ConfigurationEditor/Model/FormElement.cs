using System.Text.Json.Serialization;

namespace FEAR.Blazor.Shared.Components.ConfigurationEditor.Model
{
    public abstract class FormElement
    {
        [JsonIgnore]
        public abstract FormElement Parent { get; set; }
        public abstract string PropertyPath { get; }
        public string PropertyName { get; set; }
        public List<FormField> Fields { get; set; } = new List<FormField>();

        public void Sync()
        {
            foreach(var field in Fields)
            {
                field.Parent = this;
                field.Sync();
            }

            PostSync();
        }

        protected virtual void PostSync()
        {

        }
    }
}