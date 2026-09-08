namespace FEAR.Blazor.Shared.Components.ConfigurationEditor.Model
{
    public class FormFieldBoundingOption
    {
        public class FormOptionValue
        {
            public FormOptionValue() { }
            public FormOptionValue(string value, string displayName, bool isPrimitve)
            {
                Value = value; 
                DisplayName = displayName; 
                IsPrimitive = isPrimitve;
            }

            public bool IsPrimitive { get; set; }
            public string DisplayName { get; set; }
            public string Value { get; set; }
        }

        public bool IsDynamic { get; set; }
        public bool AllowCustomValues { get; set; }
        public String SourcePropertyPath { get; set; }
        public List<FormOptionValue> StaticList { get; set; }
        public List<FormOptionValue> BoundedList(EditorValueStore vs)
        {
            List<FormOptionValue> list = new List<FormOptionValue>();
            if (StaticList != null)
                list.AddRange(StaticList);

            if (IsDynamic && vs != null && !string.IsNullOrEmpty(SourcePropertyPath))
            {
                var keys = vs.GetArrayKeysOfProperty(SourcePropertyPath);
                foreach (var key in keys)
                {
                    var val = vs.GetValue<object>(key);
                    if (val != null)
                    {
                        list.Add(new FormOptionValue()
                        {
                            IsPrimitive = val.GetType().IsPrimitive || val is string,
                            DisplayName = val.ToString(),
                            Value = val.ToString()
                        });
                    }
                }
            }

            return list;
        }

        public FormFieldBoundingOption FromListOfDerviedTypes(List<Type> types)
        {
            IsDynamic = true;
            StaticList = types
                .Where(t => t.IsClass && !t.IsAbstract)
                .Select(t => new FormFieldBoundingOption.FormOptionValue()
                {
                    IsPrimitive = true,
                    DisplayName = t.Name,
                    Value = t.AssemblyQualifiedName
                }).ToList();
            return this;
        }
    }
}