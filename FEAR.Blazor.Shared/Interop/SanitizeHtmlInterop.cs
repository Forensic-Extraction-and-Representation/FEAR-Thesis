using Microsoft.JSInterop;

namespace FEAR.Blazor.Shared.Interop
{
    public class SanitizeHtmlInterop : InteropBase
    {
        public SanitizeHtmlInterop(IJSRuntime jsRuntime) : base(jsRuntime)
        {
        }

        public async Task<string> SanitizeHtmlAsync(string content)
        {
            return await InvokeAsync<string>("sanitizeHtml", content);
        }
    }
}
