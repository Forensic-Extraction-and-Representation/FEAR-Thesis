namespace FEAR.Domain.Telemetry
{
    public class FEARTelemetryDestinationConfiguration : IFEARTelemetryDestinationConfiguration
    {
        public IDictionary<string, object> Settings { get; set; } = new Dictionary<string, object>();
        public IFEARTelementryHandlerPredicates Predicates { get; set; }
        public string DestinationIdentifier { get; set; }

        public void MergeDefaultSettings(IDictionary<string, object> defaultSettings)
        {
            foreach (var kvp in defaultSettings)
            {
                if (!Settings.ContainsKey(kvp.Key))
                {
                    Settings[kvp.Key] = kvp.Value;
                }
            }
        }

        public string GetConfigurationValue(string key)
        {
            if (Settings.ContainsKey(key))
            {
                return Settings[key]?.ToString();
            }
            return null;
        }
    }
}
