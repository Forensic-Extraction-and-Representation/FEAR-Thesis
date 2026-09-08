using FEAR.Runtime.CSharpTranspile;
using FEAR.Runtime.TranspilerServices;

namespace FEAR.GFEAR.Transpiler
{
    /// <summary>
    /// Represents the context for a GFEAR transpilation operation.
    /// 
    /// This context is used when generating C# code for graph codify scripts that process data
    /// from the queue. The data can be either:
    ///   - An artifact directly (raw evidence or input data)
    ///   - The result of a collector (CFEAR) script
    /// 
    /// The <see cref="TypeManager"/> property tracks entity types, properties, and prefixes
    /// discovered or required during transpilation, ensuring that all queued data—regardless of
    /// its source—is correctly mapped and processed by the generated codifier scripts.
    /// </summary>
    public class GFEARTranspileContext : TranspileContext
    {
        /// <summary>
        /// Manages entity types, properties, and namespace prefixes for the current transpilation.
        /// </summary>
        public TypeManager TypeManager { get; set; } = new TypeManager();
    }
}
