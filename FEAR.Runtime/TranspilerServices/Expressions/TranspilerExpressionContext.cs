namespace FEAR.Runtime.TranspilerServices.Expressions
{
    /// <summary>
    /// Provides context for building and managing expression trees during the transpilation of graph codify scripts.
    ///
    /// <para>
    /// <b>TranspilerExpressionContext</b> is used in the FEAR transpiler pipeline to track the root and current
    /// <see cref="ExpressionScope"/> as expressions are parsed and generated. This enables the construction of
    /// complex, nested expressions for code generation.
    /// </para>
    /// <para>
    /// This context ensures that all queued data—regardless of its source—can be transformed into valid C# expressions
    /// for graph codify scripts, supporting both Blazor and WebAssembly scenarios.
    /// </para>
    /// </summary>
    public class TranspilerExpressionContext
    {
        /// <summary>
        /// Gets or sets the root expression scope for the current expression tree.
        /// </summary>
        public ExpressionScope Root { get; set; }

        /// <summary>
        /// Gets or sets the current expression scope being built or traversed.
        /// </summary>
        public ExpressionScope Current { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="TranspilerExpressionContext"/> class with a specified scope name.
        /// </summary>
        public TranspilerExpressionContext(string scopeName)
        {
            Root = new ExpressionScope { ScopeName = scopeName };
            Current = Root;
        }
    }
}
