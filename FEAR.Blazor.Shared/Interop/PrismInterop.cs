using Microsoft.AspNetCore.Components;
using FEAR.Blazor.Shared.Interop;
using Microsoft.JSInterop;

namespace FEAR.Blazor.Shared.Interop
{
    public class PrismInterop : InteropBase
    {
        public PrismInterop(IJSRuntime jsRuntime) : base(jsRuntime)
        {
        }
        public async Task HighlightAllAsync()
        {
            await InvokeVoidAsync("Prism.highlightAll");
        }

        public async Task<string> CheckTab(ElementReference textAreaElement, string key, ElementReference codeRef)
        {
            return await InvokeAsync<string>("checkTab", textAreaElement, key, codeRef);
        }

        public async Task SyncScroll(ElementReference codeElement, ElementReference resultElement)
        {
            await InvokeVoidAsync("syncScroll", codeElement, resultElement);
        }

        public async Task OnInput(string textAreaValue, ElementReference textAreaElement, ElementReference resultElement, ElementReference codeElement)
        {
            await InvokeVoidAsync("onInput", textAreaValue, textAreaElement, resultElement, codeElement);
        }

        public async Task<string> HighlightElementAsync(ElementReference elementRef, string codeText)
        {
            return await InvokeAsync<string>("Prism.highlightElement", elementRef);
        }
    }
}
