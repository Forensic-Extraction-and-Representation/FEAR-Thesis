using FEAR.Domain.Telemetry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FEAR.Runtime.Telemetry.Destinations
{
    [FEARTelemetryDestination("LogFile")]
    public partial class LogFileTelemetryDestination : FEARTelemetryDestinationBase
    {
        public Guid Id { get; private set; } = Guid.NewGuid();

        protected String LogDirectory { get; private set; }
        protected string LogFileName { get; private set; }
        protected bool CreateDateFolder { get; private set; }
        protected bool LogToConsole { get; private set; }

        public LogFileTelemetryDestination(IFEARTelemetryDestinationProvider owningProvider, IFEARTelemetryDestinationConfiguration config) : base(owningProvider, config)
        {
            CreateDateFolder = config.GetConfigurationValue("CreateDateFolder")?.ToLower() == "true";
            var ltcValue = config.GetConfigurationValue("LogToConsole")?.ToLower();
            LogToConsole = String.IsNullOrEmpty(ltcValue) || ltcValue == "true";
            LogDirectory = config.GetConfigurationValue("LogDirectory") ?? owningProvider.DefaultWorkingDirectory;
            
            if(CreateDateFolder)
            {
                var dateFolder = DateTime.UtcNow.ToString("yyyyMMdd");
                LogDirectory = Path.Combine(LogDirectory, dateFolder);
            }

            // Ensure directory exists
            if (!Directory.Exists(LogDirectory))
            {
                Directory.CreateDirectory(LogDirectory);
            }

            LogFileName = config.GetConfigurationValue("LogFileName") ?? $"telemetry-{Id.ToString("N").Split("-")[0]}-{DateTime.UtcNow:yyyyMMddHHmm}.log";

            // Override the default single-line formatter to preserve multi-line signal data
            // (e.g. full stack traces emitted by error signals).
            var baseFormatter = FormatSignal;
            FormatSignal = (signal) =>
            {
                bool isVerboseSignal = signal.SignalType == FEARTelemetrySignalTypeEnum.Error
                                    || signal.SignalType == FEARTelemetrySignalTypeEnum.Exception
                                    || (signal.SignalData != null && signal.SignalData.Contains('\n'));
                if (isVerboseSignal)
                {
                    var sep = new string('=', 80);
                    return $"{sep}{System.Environment.NewLine}"
                         + $"{signal.Timestamp:o} [{signal.SignalType}] [{signal.CaseIdentifier ?? "no case"}] {signal.SignalSource}{System.Environment.NewLine}"
                         + $"{signal.SignalData}{System.Environment.NewLine}"
                         + sep;
                }
                return baseFormatter(signal);
            };
        }

        public override void Dispose()
        {
            CommitSignalHistory();
            base.Dispose();
        }

        public override void ReceiveSignal(IFEARTelemetrySignal signal)
        {
            if (LogToConsole)
            {
                Console.WriteLine(FormatSignal(signal));
            }

            base.ReceiveSignal(signal);
        }

        public override void CommitSignalHistory()
        {
            var uncommittedSignals = this.UncomittedSignalHistoryEntries();

            try
            {
                if (uncommittedSignals.Count > 0)
                {
                    StringBuilder stringBuilder = new StringBuilder();
                    foreach (var uncommittedSignal in uncommittedSignals)
                    {
                        stringBuilder.AppendLine(FormatSignal(uncommittedSignal.Signal));
                    }
                    
                    using (StreamWriter sw = new StreamWriter(Path.Combine(LogDirectory, LogFileName), append: true))
                    {
                        sw.WriteLine(stringBuilder.ToString());
                    }

                    foreach (var signalHistory in uncommittedSignals)
                    {
                        signalHistory.Commit();
                    }
                }
            }
            catch (Exception ex)
            {
                // Handle exceptions (e.g., log them)
                Console.Error.WriteLine($"Error writing telemetry signals to log file: {ex.Message}");
            }
        }
    }
}