using FEAR.Blazor.Shared.Services;
using FEAR.WASM.Extensions;
using FEAR.WASM.Providers;
using FEAR.WASM.Services;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor;
using MudBlazor.Extensions;
using MudBlazor.Services;
using VDS.RDF.Query.Algebra;

namespace FEAR.WASM
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
            builder.Services.AddHttpClient("FEAR-HOSTED.API")
                .ConfigureHttpClient(client => client.BaseAddress = new Uri(hostedServer));
            //    .AddHttpMessageHandler<BaseAddressAuthorizationMessageHandler>();

            // Supply HttpClient instances that include access tokens when making requests to the server project.
            //builder.Services.AddScoped(provider =>
            //{
            //    var factory = provider.GetRequiredService<IHttpClientFactory>();
            //    return factory.CreateClient("FEAR-HOSTED.API");
            //});

            builder.Services.AddMudServices();
            builder.Services.AddMudMarkdownServices();
            builder.Services.AddMudExtensions();

            builder.Services.AddScoped<InvestigationService>();
            builder.Services.AddOmnibarServices();

            // Color map provider used by Graph and Dialogs
            builder.Services.AddScoped<IColorMapProvider, ColorMapProvider>();

            builder.Services.AddOidcAuthentication(options =>
            {
                options.ProviderOptions.ClientId = builder.Configuration["local:ClientId"];
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

            var app = builder.Build();
            app.InstantiateOmnibarServices();
            await app.RunAsync();
        }
    }
}
