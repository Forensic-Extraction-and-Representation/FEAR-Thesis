namespace FEAR.Domain.Telemetry
{
    public class FEARTelemetrySignalHistoryEntry : IFEARTelemetrySignalHistoryEntry
    {
        public IFEARTelemetrySignal Signal { get; private set; }
        public DateTime ReceivedAt { get; private set; }
        public bool IsCommitted { get; private set; }
        public FEARTelemetrySignalHistoryEntry(IFEARTelemetrySignal signal, DateTime receivedAt, bool isCommitted)
        {
            Signal = signal;
            ReceivedAt = receivedAt;
            IsCommitted = isCommitted;
        }

        public void Commit()
        {
            IsCommitted = true;
        }
    }
}
