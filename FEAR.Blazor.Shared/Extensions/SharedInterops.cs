using FEAR.Blazor.Shared.Interop;
using Microsoft.Extensions.DependencyInjection;

namespace FEAR.Blazor.Shared.Extensions
{
    public static class SharedInteropsExtensions
    {
        public static IServiceCollection AddSharedInterops(this IServiceCollection services)
        {
            services.AddScoped<BootstrapInterop>();
            services.AddScoped<PrismInterop>();
            services.AddScoped<TimeoutCallbackInterop>();
            return services;
        }
    }
}
