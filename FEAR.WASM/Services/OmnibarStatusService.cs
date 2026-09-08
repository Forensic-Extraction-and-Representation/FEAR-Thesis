using EventAggregator.Blazor;
using FEAR.WASM.Interactions;
using FEAR.Blazor.Shared.Interop;
using Microsoft.JSInterop;

namespace FEAR.WASM.Services
{
    public class OmnibarStatusService : EventSubscriberService, IHandle<StatusEvent>
    {
        private class StatusServiceState
        {
            public DateTime LastStatusTime { get; set; } = DateTime.Now;
            public bool ShouldAutoResolve => HasActiveResolvable && LastStatusTime.AddSeconds(30) < DateTime.Now;
            public bool HasActiveTimeout { get; set; } = true;
            public bool HasActiveResolvable { get; set; } = false;
            public int ActiveTimeout { get; set; } = 2200;

            public void Reset()
            {
                HasActiveTimeout = false;
                HasActiveResolvable = false;
                ActiveTimeout = 1000; // Reset to default timeout
            }
        }

        private System.Collections.Concurrent.ConcurrentQueue<StatusEvent> _statusQueue = new System.Collections.Concurrent.ConcurrentQueue<StatusEvent>();

        private readonly TimeoutCallbackInterop _timeoutCallbackInterop;
        private readonly DotNetObjectReference<OmnibarStatusService> dotNetReference;

        public StatusEvent CurrentStatus { get; private set; } = null;
        private StatusServiceState ServiceState { get; set; } = new StatusServiceState();
        public OmnibarStatusService(IEventAggregator eventAggregator, TimeoutCallbackInterop timeoutCallbackInterop) : base(eventAggregator)
        {
            Mediator.Subscribe(this);

            _timeoutCallbackInterop = timeoutCallbackInterop;
            dotNetReference = _timeoutCallbackInterop.CreateReference(this);
            _timeoutCallbackInterop.SetTimeoutAsync(dotNetReference, nameof(MessagePump), 1000);
        }

        public async Task HandleAsync(StatusEvent message)
        {
            if (message is ResolvableStatusEvent resolvableStatus)
            {
                if (resolvableStatus.IsResolved)
                {
                    // If the message is resolved, reset the state
                    if (CurrentStatus is ResolvableStatusEvent currentResolvable &&
                       currentResolvable.ResolveIdentifier == resolvableStatus.ResolveIdentifier)
                    {
                        // If the current status is the same resolvable status, reset it
                        ServiceState.Reset();
                    }
                    else
                    {
                        // Try find it in the queue and remove it
                        var items = _statusQueue.Where(x => x is ResolvableStatusEvent res && res.ResolveIdentifier == resolvableStatus.ResolveIdentifier).ToList();
                        foreach (var item in items)
                        {
                            _statusQueue.TryDequeue(out _);
                        }
                    }

                    TimeoutStatusEvent resolvedTimeout = new TimeoutStatusEvent
                    {
                        StatusText = resolvableStatus.StatusText,
                        Timeout = 1000 // Reset to default timeout
                    };

                    // We queue a timeout message as an update to the status
                    _statusQueue.Enqueue(resolvedTimeout);
                    return;

                }
            }
            _statusQueue.Enqueue(message);
        }

        [JSInvokable("MessagePump")]
        public async Task MessagePump()
        {
            if (ServiceState.HasActiveResolvable && !ServiceState.ShouldAutoResolve)
            {
                // We're still waiting for a resolvable status to be resolved
                _timeoutCallbackInterop.SetTimeoutAsync(dotNetReference, nameof(MessagePump), ServiceState.ActiveTimeout);
                return;
            }

            ServiceState.Reset();

            // Take the first message from the queue
            if (_statusQueue.TryDequeue(out StatusEvent statusEvent))
            {
                // If the message is a timeout event, handle it accordingly
                if (statusEvent is TimeoutStatusEvent timeoutEvent)
                {
                    ServiceState.HasActiveTimeout = true;
                    ServiceState.ActiveTimeout = timeoutEvent.Timeout;
                }
                else if (statusEvent is ResolvableStatusEvent resolvableStatus)
                {
                    ServiceState.HasActiveResolvable = true;
                    ServiceState.ActiveTimeout = 800; // Default timeout for resolvable status
                }

                // Update the current status
                CurrentStatus = statusEvent;
                // Publish the status update event
                await Mediator.PublishAsync(new GenericUIEvent(GenericUIEventConstants.STATUS_UPDATE));
            }
            else
            {
                if (!ServiceState.HasActiveResolvable && !ServiceState.HasActiveTimeout)
                {
                    if (CurrentStatus != null)
                    {
                        // If no messages are in the queue, set the current status to null (idle)
                        CurrentStatus = null;
                        await Mediator.PublishAsync(new GenericUIEvent(GenericUIEventConstants.STATUS_UPDATE));
                    }
                }
            }

            _timeoutCallbackInterop.SetTimeoutAsync(dotNetReference, nameof(MessagePump), ServiceState.ActiveTimeout);
        }

        //public async Task HandleAsync(TimeoutStatusEvent message)
        //{
        //    await HandleAsync((StatusEvent)message);
        //}
        //public async Task HandleAsync(ResolvableStatusEvent message)
        //{
        //    await HandleAsync((StatusEvent)message);
        //}
    }
}
