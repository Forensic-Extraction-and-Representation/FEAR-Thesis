namespace FEAR.Domain.Telemetry
{
    public interface IFEARTelemetryDestinationConfiguration
    {
        IDictionary<string, object> Settings { get; }
        IFEARTelementryHandlerPredicates Predicates { get; }
        string DestinationIdentifier { get; }

        string? GetConfigurationValue(string key);
    }
}
