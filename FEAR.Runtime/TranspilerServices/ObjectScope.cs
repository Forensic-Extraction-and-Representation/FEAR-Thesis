namespace FEAR.Runtime.Compiler.Transpiler
{
    /// <summary>
    /// Represents a hierarchical scope for managing variables and nested scopes during transpilation or code generation.
    ///
    /// <para>
    /// <b>ObjectScope&lt;T&gt;</b> is used in the FEAR transpiler pipeline to track variable declarations, entity names,
    /// and nested scopes (such as blocks or functions) as code is generated from source scripts.
    /// </para>
    /// <para>
    /// Each scope can contain its own variables, a reference to its parent scope, and a list of child scopes.
    /// This enables structured and context-aware code generation, supporting features like variable shadowing,
    /// block scoping, and entity-specific context.
    /// </para>
    /// </summary>
    /// <typeparam name="T">The type of variable or context object managed within the scope.</typeparam>
    public class ObjectScope<T>
    {
        /// <summary>
        /// Gets or sets the name of the scope (e.g., block label, function name, or context identifier).
        /// </summary>
        public string ScopeName { get; set; }

        /// <summary>
        /// Gets or sets the name of the entity associated with this scope, if any.
        /// </summary>
        public string EntityName { get; set; }

        /// <summary>
        /// Gets or sets the list of variables or context objects declared in this scope.
        /// </summary>
        public List<T> Variables { get; set; }

        /// <summary>
        /// Gets or sets the list of child (nested) scopes within this scope.
        /// </summary>
        public List<ObjectScope<T>> Scopes { get; set; }

        /// <summary>
        /// Gets or sets the parent scope of this scope, or null if this is the root scope.
        /// </summary>
        public ObjectScope<T> Parent { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ObjectScope{T}"/> class.
        /// </summary>
        public ObjectScope()
        {
            Variables = new List<T>();
            Scopes = new List<ObjectScope<T>>();
        }
    }
}
