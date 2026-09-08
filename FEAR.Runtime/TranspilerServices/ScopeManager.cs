namespace FEAR.Runtime.Compiler.Transpiler
{
    /// <summary>
    /// Manages a hierarchy of scopes and object contexts during transpilation.
    /// Provides functionality to add and navigate scopes, track variables (object contexts), and perform lookups
    /// for property names and types within nested scopes. Useful for managing symbol tables or variable resolution
    /// in code generation and transpiler scenarios.
    /// </summary>
    /// <typeparam name="T">The type of object context, typically representing a variable or property (e.g., <see cref="ObjectContext"/>).</typeparam>
    /// <typeparam name="R">The type of scope, typically derived from <see cref="ObjectScope{T}"/>.</typeparam>
    public class ScopeManager<T, R>
        where T : ObjectContext, new()
        where R : ObjectScope<T>, new()
    {
        /// <summary>
        /// The root scope in the hierarchy.
        /// </summary>
        public R Root { get; protected set; }

        /// <summary>
        /// The current (innermost) scope.
        /// </summary>
        public R Current { get; protected set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ScopeManager{T, R}"/> class and sets up the root scope.
        /// </summary>
        public ScopeManager()
        {
            Root = new R();
            Current = Root;
        }

        /// <summary>
        /// Adds a new child scope to the current scope and makes it the current scope.
        /// </summary>
        /// <param name="scopeName">The name of the new scope.</param>
        /// <param name="entityName">The entity name associated with the new scope.</param>
        public void AddScope(string scopeName, string entityName)
        {
            var scope = new R { ScopeName = scopeName, EntityName = entityName, Parent = Current };
            Current.Scopes.Add(scope);
            Current = scope;
        }

        /// <summary>
        /// Exits the current scope and returns to its parent scope.
        /// </summary>
        public void ExitScope()
        {
            Current = (R)Current.Parent;
        }

        /// <summary>
        /// Adds a new object context (variable/property) to the current scope.
        /// </summary>
        /// <param name="propertyName">The property or variable name.</param>
        /// <param name="type">The type (e.g., RDF type) of the property or variable.</param>
        /// <returns>The current scope after adding the object context.</returns>
        public R AddObjectContext(string propertyName, string type)
        {
            var context = new T { PropertyName = propertyName, Type = type };
            Current.Variables.Add(context);
            return Current;
        }

        /// <summary>
        /// Checks if an object context with the given property name exists in the current scope or any parent scope.
        /// </summary>
        /// <param name="propertyName">The property or variable name to look for.</param>
        /// <returns>True if the object context exists; otherwise, false.</returns>
        public bool ObjectContextExists(string propertyName)
        {
            return ObjectContextExists(propertyName, Current);
        }

        private bool ObjectContextExists(string propertyName, R scope)
        {
            if (scope.Variables.Any(x => x.PropertyName == propertyName))
                return true;

            if (scope.Parent != null)
                return ObjectContextExists(propertyName, (R)scope.Parent);

            return false;
        }

        /// <summary>
        /// Retrieves the object context with the given property name from the current scope or any parent scope.
        /// </summary>
        /// <param name="sourceName">The property or variable name to look for.</param>
        /// <returns>The matching object context if found; otherwise, null.</returns>
        public T GetObjectContext(string sourceName)
        {
            return GetObjectContext(sourceName, Current);
        }

        /// <summary>
        /// Recursively retrieves the object context with the given property name from the specified scope or its parent scopes.
        /// </summary>
        /// <param name="sourceName"></param>
        /// <param name="scope"></param>
        /// <returns></returns>
        private T GetObjectContext(string sourceName, R scope)
        {
            if (scope.Variables.Any(x => x.PropertyName == sourceName))
                return scope.Variables.First(x => x.PropertyName == sourceName);

            if (scope.Parent != null)
                return GetObjectContext(sourceName, (R)scope.Parent);

            return default(T);
        }
    }
}
