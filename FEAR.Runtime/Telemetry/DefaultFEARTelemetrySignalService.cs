using FEAR.Domain.Telemetry;
using FEAR.Runtime.Execution;

namespace FEAR.Runtime.Telemetry
{
    public class DefaultFEARTelemetrySignalService : IFEARTelemetrySignalService
    {
        private IFEARTelemetryDestinationProvider _destinationProvider;
        private String DefaultSignalSource { get; set; }
        private String CaseIdentifier { get; set; }

        public DefaultFEARTelemetrySignalService(IFEARTelemetryDestinationProvider destinationProvider, FearExecutionOptions options)
        {
            _destinationProvider = destinationProvider;
            DefaultSignalSource = options.DefaultTelemetryServiceSource;
            CaseIdentifier = options.CaseIdentifier;
        }

        public Guid Id => throw new NotImplementedException();

        public void SendSignal(IFEARTelemetrySignal signal)
        {
            // Automatically stamp the case identifier on every signal if not already set
            if (string.IsNullOrWhiteSpace(signal.CaseIdentifier))
                signal.CaseIdentifier = CaseIdentifier;

            var destinations = _destinationProvider.GetDestinations();
            bool hasHandled = false;
            foreach (var destination in destinations.Values)
            {
                if (destination.HandlesSignal(signal))
                {
                    destination.ReceiveSignal(signal);
                    hasHandled = true;
                }
            }

            if (!hasHandled)
            {
                signal.SignalSource = DefaultSignalSource;
                foreach (var destination in destinations.Values)
                {
                    if (destination.HandlesSignal(signal))
                    {
                        destination.ReceiveSignal(signal);
                        hasHandled = true;
                    }
                }
            }
        }
    }
}
