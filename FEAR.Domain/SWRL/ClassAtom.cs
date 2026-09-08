using System.Text;

namespace FEAR.Domain.SWRL
{
    /// <summary>
    /// Represents a SWRL class atom, which asserts that a variable is an instance of a given class (IRI).
    /// Generates a SPARQL triple pattern of the form "?variable a <ClassIRI> ."
    /// </summary>
    public class ClassAtom : Atom
    {
        /// <summary>
        /// Gets the name of the variable representing the instance.
        /// Returns "UnknownClass" if no variable is present.
        /// </summary>
        public string ClassName => Variables.FirstOrDefault()?.Name ?? "UnknownClass";

        /// <summary>
        /// Converts this class atom to its SPARQL string representation.
        /// </summary>
        /// <returns>A SPARQL triple pattern string.</returns>
        public override string ToSparqlString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append($"{ClassName} a {Iri} .");
            return sb.ToString();
        }
    }
}
