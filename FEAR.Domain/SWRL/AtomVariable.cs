namespace FEAR.Domain.SWRL
{
    /// <summary>
    /// Represents a variable used in a SWRL atom.
    /// An <see cref="AtomVariable"/> can be a standard variable, a string literal, or a numeric literal,
    /// as indicated by its <see cref="Type"/>. The <see cref="Name"/> property holds the variable's identifier
    /// or literal value, depending on the variable type.
    /// </summary>
    public class AtomVariable
    {
        /// <summary>
        /// Gets or sets the name or value of the variable.
        /// For standard variables, this is the variable's identifier; for literals, this is the literal value.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the type of the variable (variable, string literal, or numeric literal).
        /// </summary>
        public VariableTypeEnum Type { get; set; }
    }
}
