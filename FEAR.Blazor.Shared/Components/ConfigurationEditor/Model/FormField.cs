using System.Text.Json;
using System.Text.Json.Serialization;

namespace FEAR.Blazor.Shared.Components.ConfigurationEditor.Model
{
    public class FormField : FormElement
    {
        [JsonIgnore]
        public override FormElement Parent { get; set; }

        public FormField() { }
        public FormField(FormElement parent)
        {
            Parent = parent;
        }

        public override string PropertyPath => $"{Parent.PropertyPath}.{PropertyName}";
        public string DisplayName { get; set; }
        public string Description { get; set; }
        public string HelpLink { get; set; }
        public string Placeholder { get; set; }

        public FieldTypeEnum Type { get; set; }
        public bool IsArray { get; set; }

        public bool IsDictionary { get; set; }
        public FormField DictionaryKeyFormField => IsDictionary ? Fields.FirstOrDefault(f => f.PropertyName == DictionaryKey) : null;
        public List<FormField> DictionaryValueFormFields => IsDictionary ? Fields.Where(f => f.PropertyName != DictionaryKey).ToList() : null;

        public VisibleConditionOption VisibleCondition { get; set; }
        public bool HasVisibleCondition => VisibleCondition != null ;
        public FormFieldValidationOption ValidationOption { get; set; }
        public FormFieldBoundingOption BoundingOption { get; set; }
        public bool IsBoundedList => BoundingOption != null;

        public string DictionaryKey { get; set; }
        public bool IsFreeFieldsAllowed { get; set; }

        protected override void PostSync()
        {
            if (HasVisibleCondition)
            {
                List<object> expectedValues = new List<object>();
                foreach(var val in VisibleCondition.ExpectedValues)
                {
                    if (val is JsonElement je)
                    {
                        switch (je.ValueKind)
                        {
                            case JsonValueKind.String:
                                expectedValues.Add(je.GetString());
                                break;
                            case JsonValueKind.Number:
                                if (je.TryGetInt32(out int intValue))
                                {
                                    expectedValues.Add(intValue);
                                }
                                else if (je.TryGetInt64(out long longValue))
                                {
                                    expectedValues.Add(longValue);
                                }
                                else if (je.TryGetDouble(out double doubleValue))
                                {
                                    expectedValues.Add(doubleValue);
                                }
                                break;
                            case JsonValueKind.True:
                            case JsonValueKind.False:
                                expectedValues.Add(je.GetBoolean());
                                break;
                            default:
                                expectedValues.Add(je.ToString());
                                break;
                        }
                    }
                    else
                    {
                        expectedValues.Add(val);
                    }
                }

                VisibleCondition.ExpectedValues = expectedValues.ToArray();
            }
        }
    }
}