using FEAR.Domain.KnowledgeGraph.GraphCodify;
using System.Text.RegularExpressions;

namespace FEAR.Domain.KnowledgeGraph
{
    /// <summary>
    /// Provides utilities for formatting IRI segments using dynamic data.
    /// </summary>
    public static class IriSegmentFormatter
    {
        /// <summary>
        /// Replaces placeholders in the IRI segment string with values from the provided <see cref="CodifierExpandoObject"/>.
        /// Placeholders are specified in curly braces, e.g., "{propertyName}".
        /// If a property is not found in the data, it is replaced with an empty string.
        /// </summary>
        /// <param name="iriSegment">The IRI segment template containing placeholders.</param>
        /// <param name="data">A <see cref="CodifierExpandoObject"/> containing property values for substitution.</param>
        /// <returns>The formatted IRI segment with placeholders replaced by actual values.</returns>
        public static string Parse(string iriSegment, CodifierExpandoObject data)
        {
            var formattedIri = Regex.Replace(iriSegment, @"{(?<exp>[^}]+)}", match => {
                return ((data[match.Groups["exp"].Value] as object).ToString() ?? "");
            });
            return formattedIri;
        }
    }
}