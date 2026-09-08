namespace FEAR.Domain.Telemetry
{
    public abstract class FEARTelemetrySignalBase : IFEARTelemetrySignal
    {
        public Guid Id { get; private set; } = Guid.NewGuid();
        public abstract FEARTelemetrySignalTypeEnum SignalType { get; }
        public String SignalSource { get; set; }
        public DateTime Timestamp { get; private set; } = DateTime.UtcNow;
        public string CaseIdentifier { get; set; }

        public FEARTelemetrySerializationEnum Serialization { get; private set; }

        public string SignalData { get; private set; }

        public FEARTelemetrySignalBase(string signalSource, FEARTelemetrySerializationEnum serialization)
        {
            SignalSource = signalSource;
            Serialization = serialization;
        }

        public FEARTelemetrySignalBase WithSignalData(string signalData)
        {
            SignalData = signalData;
            return this;
        }

        public FEARTelemetrySignalBase WithCaseIdentifier(string caseIdentifier)
        {
            CaseIdentifier = caseIdentifier;
            return this;
        }
    }
}
