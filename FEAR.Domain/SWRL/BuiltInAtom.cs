using System.Text;

namespace FEAR.Domain.SWRL
{
    /// <summary>
    /// Represents a SWRL built-in atom, which maps SWRL built-in functions (such as comparisons) to their SPARQL equivalents.
    /// This class supports a set of standard comparison functions and generates the corresponding SPARQL string.
    /// </summary>
    public class BuiltInAtom : Atom
    {
        /// <summary>
        /// Maps SWRL built-in function names to their SPARQL operator equivalents.
        /// </summary>
        protected readonly Dictionary<string, string> builtInFunctions = new Dictionary<string, string>
        {
            { "greaterThan", ">" },
            { "greaterThanOrEqual", ">=" },
            { "lessThan", "<" },
            { "lessThanOrEqual", "<=" },
            { "equal", "=" },
            { "notEqual", "!=" }
        };

        /// <summary>
        /// Converts this built-in atom to its SPARQL string representation.
        /// Only supported built-in functions are translated; otherwise, a <see cref="NotSupportedException"/> is thrown.
        /// </summary>
        /// <returns>A SPARQL string representing this built-in atom.</returns>
        public override string ToSparqlString()
        {
            StringBuilder sb = new StringBuilder();
            var functionName = builtInFunctions.FirstOrDefault(kv => Iri.EndsWith(kv.Key));
            if (functionName.Key != null)
            {
                sb.Append($"{Variables[0].Name} {functionName.Value} {Variables[1].Name}".Replace("\"", "\"\""));
            }
            else
            {
                throw new NotSupportedException($"Built-in function '{Iri}' is not supported.");
            }

            return sb.ToString();
        }
    }
}
