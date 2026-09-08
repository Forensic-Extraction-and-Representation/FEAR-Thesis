using Microsoft.JSInterop;

namespace FEAR.Blazor.Shared.Interop
{
    public abstract class InteropBase
    {
        protected readonly IJSRuntime JSRuntime;
        protected InteropBase(IJSRuntime jsRuntime)
        {
            JSRuntime = jsRuntime;
        }
        protected async Task<T> InvokeAsync<T>(string methodName, params object[] args)
        {
            return await JSRuntime.InvokeAsync<T>(methodName, args);
        }
        protected async Task InvokeVoidAsync(string methodName, params object[] args)
        {
            await JSRuntime.InvokeVoidAsync(methodName, args);
        }
    }
}
