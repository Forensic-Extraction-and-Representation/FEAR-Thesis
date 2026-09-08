using Microsoft.Extensions.Configuration;

namespace FEAR.Domain.Configuration
{
    public static class DelimitedParameterParser
    {
        public static Dictionary<string, string> Parse(string delimitedParameters, string setDelimiter = ";", string keyValueDelimiter = "=")
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            string[] parts = delimitedParameters.Split(setDelimiter);
            foreach (var part in parts)
            {
                var keyValue = part.Split(keyValueDelimiter);
                if (keyValue.Length == 2)
                {
                    parameters[keyValue[0].Trim()] = keyValue[1].Trim();
                }
            }
            return parameters;
        }

        public static void ParseSectionIntoDictionary(string delimitedParameters, Dictionary<string, object> dict)
        {
            var parsedParameters = Parse(delimitedParameters);
            foreach (var kvp in parsedParameters)
            {
                if (dict.ContainsKey(kvp.Key))
                {
                    dict[kvp.Key] = kvp.Value;
                }
            }
        }
    }
}
