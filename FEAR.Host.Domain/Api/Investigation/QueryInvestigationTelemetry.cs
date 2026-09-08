using FEAR.Domain.Telemetry;

namespace FEAR.Host.Domain.Api.Investigation
{
    public class QueryInvestigationTelemetry
    {
        public class Request
        {
            public string InvestigationName { get; set; }
            public Guid InvestigationId { get; set; }
            public DateTime? Since { get; set; }
            public int? Count { get; set; } = 100;
        }

        public class TelemetrySignalDto
        {
            public DateTime Timestamp { get; set; }
            public FEARTelemetrySignalTypeEnum SignalType { get; set; }
            public string SignalSource { get; set; }
            public string SignalData { get; set; }
        }

        public class Response
        {
            public List<TelemetrySignalDto> Signals { get; set; } = new();
            public int TotalCount { get; set; }
        }
    }
}