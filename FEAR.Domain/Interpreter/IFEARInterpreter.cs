namespace FEAR.Domain.Interpreter
{
    /// <summary>
    /// Defines a contract for FEAR interpreters, which are responsible for executing logic in a given interpreter context.
    /// Interpreters can be dynamically discovered and registered, and are identified by their name and category.
    /// </summary>
    public interface IFEARInterpreter
    {
        /// <summary>
        /// Executes the interpreter logic using the provided context.
        /// </summary>
        /// <param name="context">The interpreter context containing data, state, and return values for execution.</param>
        void Execute(IInterpreterContext context);

        /// <summary>
        /// Gets the name of the interpreter, used for identification and discovery.
        /// </summary>
        string InterpreterName { get; }

        /// <summary>
        /// Gets the category of the interpreter, typically representing a logical grouping or namespace.
        /// </summary>
        string InterpreterCategory { get; }
    }
}