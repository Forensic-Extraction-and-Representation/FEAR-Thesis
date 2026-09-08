using Microsoft.JSInterop;

namespace FEAR.Blazor.Shared.Interop
{
    public class TimeoutCallbackInterop : InteropBase
    {
        public TimeoutCallbackInterop(IJSRuntime jsRuntime) : base(jsRuntime)
        {
        }

        public DotNetObjectReference<T> CreateReference<T>(T o)
            where T : class
        {
            return DotNetObjectReference.Create(o);
        }

        public async Task<int> SetTimeoutAsync<T>(DotNetObjectReference<T> referenceObject, string functionName, int timeout)
            where T : class
        {
            return await InvokeAsync<int>("setTimeoutCallback", referenceObject, functionName, timeout);
        }

        public async Task ClearTimeout(int timeoutId)
        {
            await InvokeVoidAsync("clearTimeout", timeoutId);
        }

        public async Task<int> SetIntervalAsync<T>(DotNetObjectReference<T> referenceObject, string functionName, int interval)
            where T : class
        {
            return await InvokeAsync<int>("setIntervalCallback", referenceObject, functionName, interval);
        }

        public async Task ClearInterval(int intervalId)
        {
            await InvokeVoidAsync("clearInterval", intervalId);
        }
    }
}
