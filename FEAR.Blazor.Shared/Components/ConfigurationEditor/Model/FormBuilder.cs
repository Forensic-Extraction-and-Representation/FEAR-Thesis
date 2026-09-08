namespace FEAR.Blazor.Shared.Components.ConfigurationEditor.Model
{
    public class FormBuilder
    {
        public FormRoot Root { get; set; }
        public List<FormFieldBuilder> FieldBuilders { get; set; } = new List<FormFieldBuilder>();
        public FormBuilder(FormRoot root)
        {
            Root = root;
        }

        public EditorOptions ToEditorOptions(string title)
        {
            EditorOptions editorOptions = new EditorOptions();
            editorOptions.Root = Root;
            editorOptions.Title = title;

            editorOptions.Root.Fields = FieldBuilders.Select(fb => fb.Build(editorOptions.Root)).ToList();

            return editorOptions;
        }

        public FormBuilder WithField(FormFieldBuilder fieldBuilder)
        {
            FieldBuilders.Add(fieldBuilder);
            return this;
        }
    }
}