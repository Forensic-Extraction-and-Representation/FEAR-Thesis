using EventAggregator.Blazor;
using FEAR.WASM.Interactions;
using FEAR.WASM.Services;
using Microsoft.AspNetCore.Components;

namespace FEAR.WASM.Components.Omnibar
{
    public partial class OmnibarStatusComponent:  IHandle<GenericUIEvent>
    {
        [Inject] OmnibarStatusService OmnibarStatusService { get; set; }

        protected StatusEvent CurrentStatusEvent => OmnibarStatusService.CurrentStatus;

        private static readonly List<Type> TimeoutEvents = new List<Type> { typeof(ResolvableStatusEvent), typeof(TimeoutStatusEvent) };

        protected bool IsInfoIcon => CurrentStatusEvent == null ? false : CurrentStatusEvent.GetType() == typeof(StatusEvent);
        protected bool IsIdleIcon => CurrentStatusEvent == null;
        protected bool IsTimeoutIcon => CurrentStatusEvent == null ? false : TimeoutEvents.Contains(CurrentStatusEvent.GetType());

        public Task HandleAsync(GenericUIEvent message)
        {
            if(message.Action == GenericUIEventConstants.STATUS_UPDATE)
            {
                StateHasChanged();
            }

            return Task.CompletedTask;
        }


    }
}