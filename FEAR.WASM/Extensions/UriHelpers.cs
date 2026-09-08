using System.Collections.Generic;

namespace FEAR.WASM.Helpers
{
    public static class UriHelper
    {
        /// <summary>
        /// Converts a full URI to a prefixed form using the provided prefixes dictionary.
        /// </summary>
        /// <param name="uri">The full URI to shrink.</param>
        /// <param name="prefixes">A dictionary mapping prefix names to namespace URIs.</param>
        /// <returns>The prefixed URI if a match is found, otherwise the original URI.</returns>
        public static string ShrinkUri(string uri, IDictionary<string, string> prefixes)
        {
            if (string.IsNullOrEmpty(uri) || prefixes == null)
                return uri;

            foreach (var kvp in prefixes)
            {
                if (uri.StartsWith(kvp.Value))
                {
                    return uri.Replace(kvp.Value, kvp.Key + ":");
                }
            }
            return uri;
        }
    }
}