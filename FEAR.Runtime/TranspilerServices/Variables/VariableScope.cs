using FEAR.Runtime.Compiler.Transpiler;

namespace FEAR.Runtime.TranspilerServices.Variables
{
    /// <summary>
    /// Represents a variable scope for the transpiler, tracking variables and entity context within a block or function.
    ///
    /// <para>
    /// <b>VariableScope</b> extends <see cref="ObjectScope{VariableContext}"/> to provide additional context for variable management
    /// during the transpilation of graph codify scripts. Each scope maintains its own set of variables and can reference a parent scope,
    /// supporting nested and hierarchical code generation.
    /// </para>
    /// </summary>
    public class VariableScope : ObjectScope<VariableContext>
    {
        /// <summary>
        /// Gets or sets the variable context for the current entity being processed in this scope.
        /// This enables correct mapping of properties and relationships to the entity during code generation.
        /// </summary>
        public VariableContext CurrentEntityVariableContext { get; set; }
    }
}
