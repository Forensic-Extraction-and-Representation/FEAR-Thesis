using FEAR.Runtime.Compiler.Transpiler;

namespace FEAR.Runtime.TranspilerServices.Variables
{
    /// <summary>
    /// Manages variable contexts and scoping during the transpilation of graph codify scripts.
    ///
    /// <para>
    /// <b>VariableManager</b> extends <see cref="ScopeManager{VariableContext, VariableScope}"/> to provide specialized
    /// handling for variables encountered in the FEAR transpiler pipeline. It tracks variables across nested scopes,
    /// supports adding variables to the current or root scope, and enables retrieval of result variables.
    /// </para>
    /// </summary>
    public class VariableManager : ScopeManager<VariableContext, VariableScope>
    {
        /// <summary>
        /// Adds a variable to the current scope. The current scope is related to the entity block that is being processed.
        /// </summary>
        /// <param name="sourceName">The original name of the variable (as in the source script).</param>
        /// <param name="translatedName">The name to use in the generated code.</param>
        /// <param name="source">The origin of the variable (e.g., Variable, AcceptData, or Result).</param>
        /// <returns>The created <see cref="VariableContext"/>.</returns>
        public VariableContext AddVariable(string sourceName, string translatedName, VariableContext.VariableSource source)
        {
            var variable = new VariableContext { PropertyName = sourceName, TranslatedName = translatedName, Source = source };
            Current.Variables.Add(variable);
            return variable;
        }

        /// <summary>
        /// Adds a variable to the root scope. This adds the variable to the top-level (result) scope, which is the
        /// main entity constructed by the codify script.
        /// </summary>
        /// <param name="sourceName">The original name of the variable (as in the source script).</param>
        /// <param name="translatedName">The name to use in the generated code.</param>
        /// <param name="source">The origin of the variable (e.g., Variable, AcceptData, or Result).</param>
        /// <returns>The created <see cref="VariableContext"/>.</returns>
        public VariableContext AddRootVariable(string sourceName, string translatedName, VariableContext.VariableSource source)
        {
            var variable = new VariableContext { PropertyName = sourceName, TranslatedName = translatedName, Source = source };
            Root.Variables.Add(variable);
            return variable;
        }

        /// <summary>
        /// Retrieves the variable context marked as a result in the root scope.
        /// </summary>
        /// <returns>The <see cref="VariableContext"/> with <see cref="VariableContext.VariableSource.Result"/> source.</returns>
        public VariableContext GetResultVariable()
        {
            return Root.Variables.First(x => x.Source == VariableContext.VariableSource.Result);
        }
    }
}
