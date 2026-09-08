namespace FEAR.Domain.SWRL
{
    /// <summary>
    /// Represents an abstract SWRL atom, which is a basic building block of SWRL rules.
    /// An atom consists of an IRI (predicate or class) and a list of variables (arguments).
    /// Derived classes must implement <see cref="ToSparqlString"/> to provide a SPARQL representation of the atom.
    /// </summary>
    public abstract class Atom
    {
        /// <summary>
        /// Gets or sets the IRI (predicate or class) for this atom.
        /// </summary>
        public string Iri { get; set; }

        /// <summary>
        /// Gets or sets the list of variables (arguments) for this atom.
        /// </summary>
        public List<AtomVariable> Variables { get; set; } = new List<AtomVariable>();

        /// <summary>
        /// Converts this atom to its SPARQL string representation.
        /// Derived classes must implement this to provide the correct SPARQL syntax for the atom type.
        /// </summary>
        /// <returns>A SPARQL string representing this atom.</returns>
        public abstract string ToSparqlString();
    }
}