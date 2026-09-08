namespace FEAR.Blazor.Shared.Components.ConfigurationEditor.Model
{
    public class FormFieldBuilder
    {
        public FormField Field { get; set; }
        public List<FormFieldBuilder> ChildFieldBuilders { get; set; } = new List<FormFieldBuilder>();

        public FormFieldBuilder(string displayName, string propertyName, FieldTypeEnum? fieldTypeEnum = null)
        {
            Field = new FormField()
            {
                DisplayName = displayName,
                PropertyName = propertyName
            };

            if (fieldTypeEnum.HasValue)
            {
                Field.Type = fieldTypeEnum.Value;
            }
        }

        public FormFieldBuilder WithDescription(string description)
        {
            Field.Description = description;
            return this;
        }

        public FormFieldBuilder WithVisibleOption(VisibleConditionOption visibleCondition)
        {
            Field.VisibleCondition = visibleCondition;
            return this;
        }

        public FormFieldBuilder WithHelpLink(string helpLink)
        {
            Field.HelpLink = helpLink;
            return this;
        }

        public FormFieldBuilder WithPlaceholder(string placeholder)
        {
            Field.Placeholder = placeholder;
            return this;
        }
        public FormFieldBuilder WithValidationOption(FormFieldValidationOption validationOption)
        {
            Field.ValidationOption = validationOption;
            return this;
        }
        public FormFieldBuilder AllowFreeFields()
        {
            Field.IsFreeFieldsAllowed = true;
            return this;
        }

        public FormFieldBuilder AsDictionary(string keyPropertyName)
        {
            Field.IsDictionary = true;
            Field.IsArray = false;
            Field.DictionaryKey = keyPropertyName;
            Field.Type = FieldTypeEnum.Object;
            return this;
        }

        public FormFieldBuilder AsArray(FieldTypeEnum fieldTypeEnum)
        {
            Field.Type = fieldTypeEnum;
            Field.IsDictionary = false;
            Field.IsArray = true;
            return this;
        }

        public FormFieldBuilder AsValue(FieldTypeEnum type)
        {
            Field.IsDictionary = false;
            Field.IsArray = false;
            Field.Type = type;
            return this;
        }

        public FormFieldBuilder AsObject()
        {
            Field.IsDictionary = false;
            Field.IsArray = false;
            Field.Type = FieldTypeEnum.Object;
            Field.Fields = new List<FormField>();
            return this;
        }

        public FormFieldBuilder WithField(FormFieldBuilder fieldBuilder)
        {
            ChildFieldBuilders.Add(fieldBuilder);
            return this;
        }

        public FormFieldBuilder WithBoundingOptions(FormFieldBoundingOption boundingOption)
        {
            Field.BoundingOption = boundingOption;
            return this;
        }

        public FormField Build(FormElement parent)
        {
            Field.Parent = parent;
            Field.Fields = ChildFieldBuilders.Select(fb => fb.Build(Field)).ToList();
            return Field;
        }
    }
}