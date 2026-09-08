using FEAR.Admin.Services.Management;
using FEAR.Blazor.Shared.Extensions;
using FEAR.Blazor.Shared.Services;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor.Services;

namespace FEAR.Admin
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);
            builder.RootComponents.Add<App>("#app");
            builder.RootComponents.Add<HeadOutlet>("head::after");

            builder.Services.AddScoped<BaseAddressAuthorizationMessageHandler>();
            var hostedServer = builder.Configuration["local:Authority"];

            builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
            builder.Services.AddHttpClient("FEAR-HOSTED.API")
                .ConfigureHttpClient(client => client.BaseAddress = new Uri(hostedServer));

            builder.Services.AddMudServices();
            builder.Services.AddSharedInterops();

            builder.Services.AddScoped<HttpFactory>();
            builder.Services.AddScoped<UserService>();
            builder.Services.AddScoped<RoleService>();
            builder.Services.AddScoped<ColorMapAdminService>();
            builder.Services.AddScoped<InvestigationService>();
            builder.Services.AddScoped<OidcService>();

            builder.Services.AddOidcAuthentication(options =>
            {
                options.ProviderOptions.ClientId = "Default Application";
                options.ProviderOptions.Authority = hostedServer;
                options.ProviderOptions.ResponseType = "code";

                // Note: response_mode=fragment is the best option for a SPA. Unfortunately, the Blazor WASM
                // authentication stack is impacted by a bug that prevents it from correctly extracting
                // authorization error responses (e.g error=access_denied responses) from the URL fragment.
                // For more information about this bug, visit https://github.com/dotnet/aspnetcore/issues/28344.
                //
                options.ProviderOptions.ResponseMode = "query";
                options.AuthenticationPaths.RemoteRegisterPath = $"{hostedServer}Identity/Account/Register";

                // Add the "roles" (OpenIddictConstants.Scopes.Roles) scope and the "role" (OpenIddictConstants.Claims.Role) claim
                // (the same ones used in the Startup class of the Server) in order for the roles to be validated.
                // See the Counter component for an example of how to use the Authorize attribute with roles
                options.ProviderOptions.DefaultScopes.Add("roles");
                options.UserOptions.RoleClaim = "role";
            });

            await builder.Build().RunAsync();
        }
    }
}
