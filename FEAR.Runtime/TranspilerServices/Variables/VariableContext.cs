using FEAR.Runtime.Compiler.Transpiler;

namespace FEAR.Runtime.TranspilerServices.Variables
{
    /// <summary>
    /// Represents the context for a variable during transpilation of graph codify scripts.
    ///
    /// <para>
    /// <b>VariableContext</b> extends <see cref="ObjectContext"/> to provide additional metadata about variables
    /// encountered in the transpiler pipeline. This includes the variable's source (e.g., declared variable, accepted data, or result),
    /// and the translated name used in generated code.
    /// </para>
    /// <para>
    /// This context is essential for mapping, tracking, and generating correct variable references in the output C# code
    /// for graph codify scripts, ensuring that all queued data—regardless of its source—is handled consistently.
    /// </para>
    /// </summary>
    public class VariableContext : ObjectContext
    {
        /// <summary>
        /// Specifies the origin of the variable in the transpilation process.
        /// </summary>
        public enum VariableSource
        {
            /// <summary>
            /// The variable is a local or intermediate variable created during code generation.
            /// </summary>
            Variable,
            /// <summary>
            /// The variable represents data accepted as input (e.g., from queued artifacts or collector results).
            /// </summary>
            AcceptData,
            /// <summary>
            /// The variable represents a result object (e.g., the output of a collector or codifier).
            /// </summary>
            Result
        }

        /// <summary>
        /// Gets or sets the source of the variable (e.g., Variable, AcceptData, or Result).
        /// </summary>
        public VariableSource Source { get; set; }

        /// <summary>
        /// Gets or sets the translated name of the variable as it will appear in the generated code.
        /// </summary>
        public string TranslatedName { get; set; }
    }
}
