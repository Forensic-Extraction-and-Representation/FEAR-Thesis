namespace FEAR.GFEAR.Transpiler
{
    /// <summary>
    /// Provides context fields and variable resolution logic for the CSharpTranspiler.
    /// 
    /// This context is used when generating C# code for graph codify scripts that process data
    /// from the queue. The data can be either:
    ///   - An artifact directly (raw evidence or input data)
    ///   - The result of a collector (CFEAR) script
    /// 
    /// The variable resolution ensures that variables in the generated code are correctly mapped
    /// to either control block variables or to the queued data context.
    /// </summary>
    public partial class CSharpTranspiler
    {
        /// <summary>
        /// Tracks variables defined in control blocks (such as foreach loops) within the generated code.
        /// </summary>
        protected IList<string> ControlBlockVariables = new List<string>();

        /// <summary>
        /// Resolves a variable name to its correct context in the generated code.
        /// If the variable is a control block variable, it is used directly.
        /// Otherwise, it is resolved from the queued data context (artifact or collector result).
        /// </summary>
        /// <param name="variableName">The variable name to resolve.</param>
        /// <returns>The resolved variable reference as a string.</returns>
        protected string ResolveVariable(string variableName)
        {
            if (ControlBlockVariables.Contains(variableName))
                return variableName;

            return $@"context.Data[""{variableName}""]";
        }
    }
}
