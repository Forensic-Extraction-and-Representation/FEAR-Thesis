using System.Text.Json;
using System.Text.Json.Nodes;

namespace FEAR.Domain.Helpers
{
    /// <summary>
    /// Provides extension/helper methods for working with JSON 
    /// nodes and elements, specifically for extracting epoch values.
    /// </summary>
    public static class JsonNodeHelpers
    {
        /// <summary>
        /// Converts the value of a <see cref="JsonElement"/> to a long integer representing an epoch time.
        /// If the value cannot be parsed, returns 0.
        /// </summary>
        /// <param name="node">The <see cref="JsonElement"/> to convert.</param>
        /// <returns>The parsed epoch value as a <see cref="long"/>, or 0 if parsing fails.</returns>
        public static long AsEpoch(this JsonElement node)
        {
            string strValue = node.GetString();
            long intValue = 0;
            long.TryParse(strValue, out intValue);
            return intValue;
        }

        /// <summary>
        /// Converts the value of a <see cref="JsonValue"/> to a long integer representing an epoch time.
        /// If the value cannot be parsed, returns 0.
        /// </summary>
        /// <param name="node">The <see cref="JsonValue"/> to convert.</param>
        /// <returns>The parsed epoch value as a <see cref="long"/>, or 0 if parsing fails.</returns>
        public static long AsEpoch(this JsonValue node)
        {
            string strValue = node.GetValue<string>();
            long intValue = 0;
            long.TryParse(strValue, out intValue);
            return intValue;
        }
    }
}