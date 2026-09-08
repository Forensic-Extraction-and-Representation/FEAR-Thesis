using EventAggregator.Blazor;
using Microsoft.AspNetCore.Components;

namespace FEAR.WASM.Components
{
    public partial class EventSubscriberBase : ComponentBase
    {
        [Inject]
        protected IEventAggregator Mediator { get; set; } = null!;

        protected override async Task OnInitializedAsync()
        {
            Mediator?.Subscribe(this);
        }
    }
}