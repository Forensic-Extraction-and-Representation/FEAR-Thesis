namespace FEAR.Runtime.Compiler.Transpiler
{
    /// <summary>
    /// Represents context information for an object during transpilation.
    /// Used to track the property name and its RDF type as the object is processed by the transpiler.
    /// </summary>
    public class ObjectContext
    {
        /// <summary>
        /// The name of the property being transpiled.
        /// </summary>
        public string PropertyName { get; set; }

        /// <summary>
        /// The RDF type (e.g., class or datatype URI) associated with the property or object.
        /// </summary>
        public string Type { get; set; }
    }
}
