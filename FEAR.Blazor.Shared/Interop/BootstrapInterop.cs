using Microsoft.JSInterop;

namespace FEAR.Blazor.Shared.Interop
{
    public class BootstrapInterop : InteropBase
    {
        public BootstrapInterop(IJSRuntime jsRuntime) : base(jsRuntime)
        {
        }

        public async Task InitializeModal(string modalId, string backdropType, string top = null, string left = null)
        {
            bool hasPosition = !string.IsNullOrEmpty(top) && !string.IsNullOrEmpty(left);

            if (hasPosition)
            {
                await JSRuntime.InvokeVoidAsync("initializeModal", "#" + modalId, backdropType, top, left);
            }
            else
            {
                await JSRuntime.InvokeVoidAsync("initializeModal", "#" + modalId, backdropType);
            }
        }

        public async Task DraggableModal(string modalId)
        {
            await JSRuntime.InvokeVoidAsync("draggableModal", "#" + modalId);
        }

        public async Task CloseModal(string modalId)
        {
            await JSRuntime.InvokeVoidAsync("closeModal", "#" + modalId);
        }
    }
}
