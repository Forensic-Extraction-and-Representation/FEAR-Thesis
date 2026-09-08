namespace FEAR.Domain.Telemetry
{
    public interface IFEARTelemetrySignal
    {
        Guid Id { get; }
        FEARTelemetrySerializationEnum Serialization { get; }
        FEARTelemetrySignalTypeEnum SignalType { get; }
        String SignalSource { get; set; }
        DateTime Timestamp { get; }
        String CaseIdentifier { get; set; }
        String SignalData { get; }
    }
}
