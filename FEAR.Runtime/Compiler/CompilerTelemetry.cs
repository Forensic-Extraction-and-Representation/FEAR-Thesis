using FEAR.Domain.Telemetry;

namespace FEAR.Runtime.Compiler
{
    public class CompilerTelemetry
    {

        public static void SendTelemetrySignal(IFEARTelemetrySignal signal, IFEARTelemetrySignalService telemetryService)
        {
            if (telemetryService != null)
            {
                telemetryService.SendSignal(signal);
            }
        }

        public static void SendCompilationTelemetrySignal(string message, IFEARTelemetrySignalService telemetryService)
        {
            SendTelemetrySignal(new GenericFEARTelemetrySignal(FEARCompiler.COMPILER_SOURCE, FEARTelemetrySerializationEnum.Raw, FEARTelemetrySignalTypeEnum.Compilations)
                .WithSignalData(message), telemetryService);
        }
    }
}
