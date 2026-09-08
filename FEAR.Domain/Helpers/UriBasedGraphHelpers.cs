using System.Text.RegularExpressions;

namespace FEAR.Domain.Helpers
{
    /// <summary>
    /// Provides helper members for working with URI-based graphs, including utilities for prefix notation.
    /// </summary>
    public static class UriBasedGraphHelpers
    {
        /// <summary>
        /// Regular expression for matching prefix notation in URIs (e.g., "rdf:type").
        /// The pattern captures a prefix and a local name, ensuring the local name does not start with "//".
        /// </summary>
        public static Regex PrefixNotationRegex = new Regex("^(.*):([^//].*)$");
    }
}
