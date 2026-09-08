namespace FEAR.Domain.Telemetry
{
    public interface IFEARTelemetryDestination : IDisposable
    {
        Guid Id { get; }
        string DestinationIdentifier { get; }
        Func<IFEARTelemetrySignal, string> FormatSignal { get; set; }
        void ConfigureFor(IFEARTelementryHandlerPredicates handlerPredicates);
        bool HandlesSignal(IFEARTelemetrySignal signal);

        void ReceiveSignal(IFEARTelemetrySignal signal);

        List<IFEARTelemetrySignalHistoryEntry> GetSignalHistory();

        List<IFEARTelemetrySignalHistoryEntry> WhereSignalHistory(Predicate<IFEARTelemetrySignalHistoryEntry> predicate);
        List<IFEARTelemetrySignal> GetSignals();

        List<IFEARTelemetrySignal> WhereSignal(Predicate<IFEARTelemetrySignal> predicate);

        List<IFEARTelemetrySignal> GetSignalsByType(FEARTelemetrySignalTypeEnum signalType);

        List<IFEARTelemetrySignal> GetLastNSignals(int n);

        List<IFEARTelemetrySignal> GetSignalsSince(DateTime since);
    }
}
