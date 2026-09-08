using EventAggregator.Blazor;
using FEAR.WASM.Interactions;
using FEAR.WASM.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace FEAR.WASM.Components.Omnibar
{
    public partial class MessageHistoryComponent : IHandle<GenericUIEvent>
    {
        [Inject] SessionHistoryService SessionHistoryService { get; set; } = default!;
        [Inject] IJSRuntime JSRuntime { get; set; } = default!;

        public async Task HandleAsync(GenericUIEvent message)
        {
            if(message.IsAction(new[] { GenericUIEventConstants.SESSION_UPDATED }))
            {
                // Handle session change events if needed
                StateHasChanged();

                ScrollToEnd("omnibar-message-wrapper");
            }
        }

        public async Task ScrollToEnd(string divId)
        {
            // Use JS interop to scroll to the bottom of the message history
            await JSRuntime.InvokeVoidAsync("scrollToBottom", divId);
        }
    }
}