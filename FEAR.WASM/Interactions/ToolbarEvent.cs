namespace FEAR.WASM.Interactions
{
    public class ToolbarEvent<T>
    {
        public ToolbarEvent(string option, T value)
        {
            Option = option;
            Value = value;
        }

        public string Option { get; set; }
        public T Value { get; set; }
    }
}
