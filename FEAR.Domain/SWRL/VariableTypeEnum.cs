namespace FEAR.Domain.SWRL
{
    /// <summary>
    /// Specifies the type of a variable used in a SWRL atom.
    /// This enumeration distinguishes between standard variables, string literals, and numeric literals
    /// for use in SWRL rules and SPARQL generation.
    /// </summary>
    public enum VariableTypeEnum
    {
        /// <summary>
        /// A standard variable (e.g., ?x, ?y) used in SWRL atoms.
        /// </summary>
        Variable,

        /// <summary>
        /// A string literal value (e.g., "example").
        /// </summary>
        Literal_String,

        /// <summary>
        /// A numeric literal value (e.g., 42, 3.14).
        /// </summary>
        Literal_Number
    }
}
