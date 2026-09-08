namespace FEAR.Domain.Telemetry
{
    public interface IFEARTelemetryDestinationProvider
    {
        IDictionary<string, IFEARTelemetryDestination> GetDestinations();
        Func<IFEARTelemetrySignal, string> FormatSignal { get; set; }
        string DefaultWorkingDirectory { get; }

        IFEARTelemetryDestination? RegisterDestination(string destinationName, string destinationIdentifier, Dictionary<string, object> settings);
        IFEARTelemetryDestination? RegisterDestination(string destinationName, string destinationIdentifier, Dictionary<string, object> settings, IFEARTelementryHandlerPredicates predicates);
    }
}
