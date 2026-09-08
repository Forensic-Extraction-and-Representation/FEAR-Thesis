using Microsoft.JSInterop;
using FEAR.Blazor.Shared.Interop;

namespace FEAR.WASM.Interop
{
    public class MarkedInterop : InteropBase
    {
        public MarkedInterop(IJSRuntime jsRuntime) : base(jsRuntime)
        {
        }
        public async Task<string> MarkedAsync(string content)
        {
            return await InvokeAsync<string>("marked.parse", content);
        }
    }
}
