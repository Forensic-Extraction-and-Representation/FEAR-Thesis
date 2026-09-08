using FEAR.Domain.Arguments;
using FEAR.Domain.Telemetry;

namespace FEAR.Runtime.Execution
{
    public class FEARHostedTelemetry
    {

        public static void SendTelemetrySignal(IFEARTelemetrySignal signal, IFEARTelemetrySignalService telemetryService)
        {
            if (telemetryService != null)
            {
                telemetryService.SendSignal(signal);
            }
        }

        public static void SendExecutionTelemetry(string message, IFEARTelemetrySignalService telemetryService, FEARTelemetrySignalTypeEnum telemetrySignalType = FEARTelemetrySignalTypeEnum.Environment)
        {
            SendTelemetrySignal(new GenericFEARTelemetrySignal(FearExecutionContext<FearArguments>.FEARHOSTED_SOURCE, FEARTelemetrySerializationEnum.Raw, telemetrySignalType)
                .WithSignalData(message), telemetryService);
        }
    }
}
