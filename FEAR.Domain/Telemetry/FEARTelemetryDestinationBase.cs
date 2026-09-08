using System.Collections.Concurrent;

namespace FEAR.Domain.Telemetry
{
    public abstract class FEARTelemetryDestinationBase : IFEARTelemetryDestination
    {
        private Thread TrimSignalHistoryTask = null;

        protected ConcurrentBag<IFEARTelemetrySignalHistoryEntry> SignalHistory { get; set; }
        protected IFEARTelementryHandlerPredicates Predicates { get; set; }
        protected int MaxSignalHistoryEntries { get; set; } = 1000;
        protected IFEARTelemetryDestinationProvider OwningProvider { get; }

        public string DestinationIdentifier { get; protected set; }
        public Guid Id { get; } = Guid.NewGuid();

        public Func<IFEARTelemetrySignal, string> FormatSignal { get; set; } = (signal) => $"{signal.Timestamp.ToString("o")}-{signal.SignalType}-[{signal.CaseIdentifier ?? "no case"}]-{signal.SignalSource}::{signal.SignalData}";
                
        public FEARTelemetryDestinationBase(IFEARTelemetryDestinationProvider owningProvider, IFEARTelemetryDestinationConfiguration config)
        {
            OwningProvider = owningProvider;
            DestinationIdentifier = config.DestinationIdentifier;
            FormatSignal = owningProvider.FormatSignal;
            Predicates = config.Predicates;
            SignalHistory = new ConcurrentBag<IFEARTelemetrySignalHistoryEntry>();

            // Create a background task to trim the signal history periodically
            TrimSignalHistoryTask = new Thread(() =>
            {
                while (true)
                {
                    try
                    {
                        for (int i = 0; i < 6; i++)
                        {
                            Thread.Sleep(TimeSpan.FromSeconds(10));
                            CommitSignalHistory();
                        }

                        TrimSignalHistory();
                    }
                    catch (ThreadInterruptedException tie)
                    {
                        // Exit the thread gracefully
                        break;
                    }
                }
            });

            TrimSignalHistoryTask.Start();
        }

        public virtual void Dispose()
        {
            CommitSignalHistory();
            TrimSignalHistoryTask.Interrupt();

        }

        public void ConfigureFor(IFEARTelementryHandlerPredicates predicates)
        {
            Predicates = predicates;
        }

        public virtual bool HandlesSignal(IFEARTelemetrySignal incomingSignal)
        {
            foreach(var predicate in Predicates.SignalPredicates)
            {
                if (predicate(incomingSignal))
                {
                    return true;
                }
            }

            return false;
        }

        public virtual void ReceiveSignal(IFEARTelemetrySignal signal)
        {
            AddSignal(signal);
        }

        public abstract void CommitSignalHistory();

        protected List<IFEARTelemetrySignalHistoryEntry> UncomittedSignalHistoryEntries()
        {
            return SignalHistory.Where(entry => !entry.IsCommitted).OrderBy(entry => entry.ReceivedAt).ToList();
        }

        protected void MarkSignalHistoryEntryAsCommitted(IFEARTelemetrySignalHistoryEntry entry)
        {
            entry.Commit();
        }

        public List<IFEARTelemetrySignalHistoryEntry> GetSignalHistory() => SignalHistory.OrderBy(entry => entry.ReceivedAt).ToList();

        public List<IFEARTelemetrySignalHistoryEntry> WhereSignalHistory(Predicate<IFEARTelemetrySignalHistoryEntry> predicate)
        {
            return SignalHistory.Where(entry => predicate(entry)).OrderBy(entry => entry.ReceivedAt).ToList();
        }

        public List<IFEARTelemetrySignal> GetSignals()
        {
            return SignalHistory.Select(entry => entry.Signal).OrderBy(signal => signal.Timestamp).ToList();
        }

        public List<IFEARTelemetrySignal> WhereSignal(Predicate<IFEARTelemetrySignal> predicate)
        {
            return SignalHistory.Where(entry => predicate(entry.Signal)).Select(entry => entry.Signal).OrderBy(signal => signal.Timestamp).ToList();
        }

        public List<IFEARTelemetrySignal> GetSignalsByType(FEARTelemetrySignalTypeEnum signalType)
        {
            return WhereSignal(signal => signal.SignalType == signalType);
        }

        public List<IFEARTelemetrySignal> GetLastNSignals(int n)
        {
            return SignalHistory.OrderByDescending(entry => entry.ReceivedAt).Take(n).Select(entry => entry.Signal).ToList();
        }

        public List<IFEARTelemetrySignal> GetSignalsSince(DateTime since)
        {
            return WhereSignal(signal => signal.Timestamp >= since);
        }

        protected virtual void AddSignal(IFEARTelemetrySignal signal)
        {
            SignalHistory.Add(new FEARTelemetrySignalHistoryEntry(signal, DateTime.UtcNow, false));
        }

        protected void TrimSignalHistory()
        {
            while (SignalHistory.Count > MaxSignalHistoryEntries)
            {
                var oldestEntry = SignalHistory.OrderBy(entry => entry.ReceivedAt).FirstOrDefault();
                if (oldestEntry != null)
                {
                    SignalHistory.TryTake(out _);
                }
            }
        }
    }
}
