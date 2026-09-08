namespace FEAR.Domain.Telemetry
{
    public enum FEARTelemetrySignalTypeEnum
    {
        Unknown = 0,
        Compilations = 1,
        Telemetry = 2,
        Metric = 3,
        Log = 4,
        Exception = 5,
        Dependency = 6,
        Trace = 7,
        Environment = 8,
        Warning = 9,
        Information = 10,
        Debug = 11,
        Error = 12,
    }
}
