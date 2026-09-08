using System.Text;

namespace FEAR.Domain.SWRL
{
    /// <summary>
    /// Represents a SWRL property atom, which asserts a relationship (property) between two variables or values.
    /// Generates a SPARQL triple pattern of the form "?subject <PropertyIRI> ?object ."
    /// </summary>
    public class PropertyAtom : Atom
    {
        /// <summary>
        /// Converts this property atom to its SPARQL string representation.
        /// </summary>
        /// <returns>A SPARQL triple pattern string representing the property relationship.</returns>
        public override string ToSparqlString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append($"{Variables[0].Name} {Iri} {Variables[1].Name} .".Replace("\"", "\"\""));
            return sb.ToString();
        }
    }
}
