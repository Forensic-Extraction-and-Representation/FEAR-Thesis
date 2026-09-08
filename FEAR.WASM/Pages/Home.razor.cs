using EventAggregator.Blazor;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace FEAR.WASM.Pages
{
    [Authorize]
    public partial class Home
    {
        [Inject] protected IJSRuntime JS { get; set; } = default!;
        [Inject] protected IEventAggregator Mediator { get; set; } = default!;
    }
}