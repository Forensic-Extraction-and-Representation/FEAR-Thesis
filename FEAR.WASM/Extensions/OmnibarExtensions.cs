using FEAR.WASM.Interop;
using FEAR.WASM.Model;
using FEAR.WASM.Services;
using FEAR.Blazor.Shared.Extensions;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.JSInterop;

namespace FEAR.WASM.Extensions
{
    public static class OmnibarExtensions
    {
        public static void InstantiateOmnibarServices(this WebAssemblyHost host)
        {
            host.Services.GetServices<OmnibarStatusService>();
            host.Services.GetServices<QueryService>();
            host.Services.GetServices<AgentAdapterService>();
            host.Services.GetServices<EndpointService>();
        }
        public static IServiceCollection AddOmnibarServices(this IServiceCollection builderCollection)
        {
            builderCollection.AddSingleton<WebUIConfiguration>();
            builderCollection.AddScoped<QueryService>();
            builderCollection.AddScoped<AgentAdapterService>();
            builderCollection.AddScoped<EndpointService>();
            builderCollection.AddScoped<SessionHistoryService>();
            builderCollection.AddScoped<OmnibarStatusService>();

            builderCollection.AddScoped<WASMInternalHttpFactory>();

            builderCollection.AddScoped<MarkedInterop>();
            builderCollection.AddSharedInterops();
            builderCollection.AddKeyedScoped<SigmaJsInterop>("MainGraphContainer", (services, _) => new SigmaJsInterop(services.GetService<IJSRuntime>(), "graph-canvas"));
            builderCollection.AddEventAggregator(cfg => cfg.AutoRefresh = true);

            return builderCollection;
        }
    }
}
