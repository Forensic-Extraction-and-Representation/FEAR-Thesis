namespace FEAR.Domain.Interpreter
{
    /// <summary>
    /// Attribute used to mark a class as a FEAR interpreter.
    /// This attribute ties a specific interpreter implementation to a name and category, enabling dynamic discovery,
    /// registration, and resolution of interpreters within the FEAR framework.
    /// The <see cref="FullName"/> property provides a unique identifier for the interpreter, combining category and name.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class FEARInterpreterAttribute : Attribute
    {
        /// <summary>
        /// Gets or sets the name of the interpreter.
        /// </summary>
        public string InterpreterName { get; set; }

        /// <summary>
        /// Gets or sets the category of the interpreter, typically representing a logical grouping or namespace.
        /// </summary>
        public string Category { get; set; }

        /// <summary>
        /// Gets the full name of the interpreter, combining category and name.
        /// </summary>
        public string FullName => $"{Category.TrimEnd('/')}/{InterpreterName}";

        /// <summary>
        /// Initializes a new instance of the <see cref="FEARInterpreterAttribute"/> class with the specified name and category.
        /// </summary>
        /// <param name="name">The interpreter name.</param>
        /// <param name="category">The interpreter category (e.g., logical grouping or namespace).</param>
        public FEARInterpreterAttribute(string name, string category)
        {
            InterpreterName = name;
            Category = category;
        }
    }
}