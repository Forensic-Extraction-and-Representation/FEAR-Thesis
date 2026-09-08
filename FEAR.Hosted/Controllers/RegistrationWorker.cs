using FEAR.Host.Core.Database;
using FEAR.Host.Core.Identity;
using OpenIddict.Abstractions;
using OpenIddict.EntityFrameworkCore.Models;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace FEAR.Hosted.Controllers
{
    public class Worker : IHostedService
    {
        private readonly IServiceProvider _serviceProvider;

        public Worker(IServiceProvider serviceProvider)
            => _serviceProvider = serviceProvider;

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            using var scope = _serviceProvider.CreateScope();

            var context = scope.ServiceProvider.GetRequiredService<OpenIdApplicationDbContext>();
            var manager = scope.ServiceProvider.GetRequiredService<IOpenIddictApplicationManager>();
            context.Database.EnsureCreated();

            var x = await manager.FindByClientIdAsync("service-worker");
            var app = x as OpenIddictEntityFrameworkCoreApplication;
            if (app is null)
            {
                await manager.CreateAsync(new OpenIddictApplicationDescriptor
                {
                    ApplicationType = ApplicationTypes.Web,
                    ClientType = ClientTypes.Public,
                    ConsentType = ConsentTypes.Explicit,
                    ClientId = "service-worker",
                    //ClientSecret = "388D45FA-B36B-4988-BA59-B187D329C207",
                    Permissions =
                    {
                        Permissions.Endpoints.Token,
                        Permissions.Endpoints.Authorization,
                        Permissions.GrantTypes.AuthorizationCode,
                        Permissions.ResponseTypes.Code,
                        Permissions.Scopes.Profile,
                        Permissions.Scopes.Roles
                    },
                    RedirectUris = {
                        new Uri("https://localhost:7181/authentication/login-callback")
                    }
                });
            }
            else
            {
                OpenIddictApplicationDescriptor applicationDescriptor = new OpenIddictApplicationDescriptor();
                var xt = manager.PopulateAsync(applicationDescriptor, app);
                await manager.PopulateAsync(app, applicationDescriptor);
                await manager.UpdateAsync(app);
            }
        }

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
}
