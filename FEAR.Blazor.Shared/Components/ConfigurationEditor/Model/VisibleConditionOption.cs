namespace FEAR.Blazor.Shared.Components.ConfigurationEditor.Model
{
    public class VisibleConditionOption
    {
        public string PropertyName { get; set; }
        public object[] ExpectedValues { get; set; }

        public bool IsFieldVisible(EditorValueStore valueStore)
        {
            if (valueStore == null || string.IsNullOrEmpty(PropertyName) || ExpectedValues == null || ExpectedValues.Length == 0)
            {
                return true; // No condition means always visible
            }

            var currentValue = valueStore.GetValue<object>(PropertyName);
            var valueExists = valueStore.PropertyExists(PropertyName);

            if (!valueExists || currentValue == null)
            {
                return false; // Property not found means not visible
            }

            var inList = ExpectedValues.Contains(currentValue);
            return inList;
        }
    }
}