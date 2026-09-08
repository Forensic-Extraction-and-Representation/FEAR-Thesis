namespace FEAR.Domain.Arguments
{
    /// <summary>
    /// Represents a parameter that can be set or unset, with a value of type T.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    public class Parameter<T>
    {
        private T _value = default(T);
        public bool IsSet { get; set; }
        public T Value { get => _value; set => _value = value; }

        public Parameter(T value)
        {
            _value = value;
            IsSet = true;
        }

        public Parameter()
        {
            IsSet = false;
        }
    }
}
