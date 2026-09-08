namespace FEAR.Domain.Telemetry
{
    public interface IFEARTelemetrySignalHistoryEntry
    {
        IFEARTelemetrySignal Signal { get; }
        DateTime ReceivedAt { get; }
        bool IsCommitted { get; }
        void Commit();
    }
}
