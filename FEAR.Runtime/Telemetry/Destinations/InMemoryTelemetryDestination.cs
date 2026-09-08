using FEAR.Domain.Telemetry;
using System.Collections.Concurrent;

namespace FEAR.Runtime.Telemetry.Destinations
{
    public partial class LogFileTelemetryDestination
    {
        [FEARTelemetryDestination("InMemory")]
        public class InMemoryTelemetryDestination : FEARTelemetryDestinationBase
        {
            public Guid Id { get; private set; } = Guid.NewGuid();
            protected ConcurrentQueue<IFEARTelemetrySignal> Signals { get; private set; } = new ConcurrentQueue<IFEARTelemetrySignal>();
            public InMemoryTelemetryDestination(IFEARTelemetryDestinationProvider owningProvider, IFEARTelemetryDestinationConfiguration config) : base(owningProvider, config)
            {
                MaxSignalHistoryEntries = int.MaxValue;
            }

            public override void CommitSignalHistory()
            {
                var uncommittedSignals = this.UncomittedSignalHistoryEntries();
                foreach (var signalHistory in uncommittedSignals)
                {
                    Signals.Enqueue(signalHistory.Signal);
                    signalHistory.Commit();
                }
            }
        }
    }
}
