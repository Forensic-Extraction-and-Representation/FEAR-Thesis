namespace FEAR.Domain.Telemetry
{
    public class GenericFEARTelemetrySignal : FEARTelemetrySignalBase
    {
        public override FEARTelemetrySignalTypeEnum SignalType { get; }

        public GenericFEARTelemetrySignal(string signalSource, FEARTelemetrySerializationEnum serialization, FEARTelemetrySignalTypeEnum signalType) : base(signalSource, serialization)
        {
            SignalType = signalType;
        }
    }
}
