using EventAggregator.Blazor;

namespace FEAR.WASM.Services
{
    public abstract class EventSubscriberService
    {
        protected IEventAggregator Mediator;

        public EventSubscriberService(IEventAggregator eventAggregator)
        {
            Mediator = eventAggregator;
            Mediator.Subscribe(this);
        }
    }
}
