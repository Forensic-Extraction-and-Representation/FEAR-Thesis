using FEAR.Host.Core.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpenIddict.Abstractions;
using OpenIddict.EntityFrameworkCore.Models;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace FEAR.Host.Core.Database
{
    public class OpenIdDbSeedProvider
    {
        private IConfiguration _configuration;
        private IOpenIddictApplicationManager _openIddictApplicationManager;
        private IOpenIddictAuthorizationManager _openIddictAuthorizationManager;
        private OpenIdApplicationDbContext _openIdDbContext;
        private IdentityDbContext _identityDbContext;

        public OpenIdDbSeedProvider(IConfiguration configuration, IServiceProvider serviceProvider)
        {
            _configuration = configuration;
        }

        public async Task SeedAsync(OpenIdApplicationDbContext openIdDbContext, IdentityDbContext identityDbContext, IOpenIddictApplicationManager openIdManager, IOpenIddictAuthorizationManager openIdAuthorizationManager, string? systemTld, bool allowDelete)
        {
            // Optionally delete the database if configured to reload seed data
            if (_configuration["DatabaseConfigurationOptions:ReloadSeedData"].ToLower() == "true")
            {
                if (allowDelete)
                    openIdDbContext.Database.EnsureDeleted();
            }

            openIdDbContext.Database.Migrate();
            _openIdDbContext = openIdDbContext;
            _identityDbContext = identityDbContext;
            _openIddictApplicationManager = openIdManager;
            _openIddictAuthorizationManager = openIdAuthorizationManager;
            await SeedData(systemTld);
        }

        private async Task SeedData(string systemTld)
        {
            var existingApp = await _openIddictApplicationManager.FindByClientIdAsync("Default Application");
            var app = existingApp as OpenIddictEntityFrameworkCoreApplication;
            if (app is null)
            {
                await _openIddictApplicationManager.CreateAsync(new OpenIddictApplicationDescriptor
                {
                    DisplayName= "Default Application",
                    ApplicationType = ApplicationTypes.Web,
                    ClientType = ClientTypes.Public,
                    ConsentType = ConsentTypes.Explicit,
                    ClientId = "Default Application",
                    Permissions =
                    {
                        Permissions.Endpoints.Token,
                        Permissions.Endpoints.Authorization,
                        Permissions.GrantTypes.AuthorizationCode,
                        Permissions.ResponseTypes.Code,
                        Permissions.Scopes.Profile,
                        Permissions.Scopes.Roles,
                        Permissions.Scopes.Email
                    },
                    RedirectUris = {
                        new Uri($"https://api.{systemTld}/authentication/login-callback"),
                        new Uri($"https://ui.{systemTld}/authentication/login-callback")
                    }
                });

                await _openIddictApplicationManager.CreateAsync(new OpenIddictApplicationDescriptor
                {
                    DisplayName = "Autopsy Application",
                    ApplicationType = ApplicationTypes.Web,
                    ClientType = ClientTypes.Public,
                    ConsentType = ConsentTypes.Explicit,
                    ClientId = "autopsy-application",
                    Permissions =
                    {
                        Permissions.Endpoints.Token,
                        Permissions.Endpoints.Authorization,
                        Permissions.GrantTypes.AuthorizationCode,
                        Permissions.ResponseTypes.Code,
                        Permissions.Scopes.Profile,
                        Permissions.Scopes.Roles,
                        Permissions.Scopes.Email
                    },
                    RedirectUris = {
                        new Uri($"fear-autopsy://internal-auth/login-callback")
                    }
                });


                existingApp = await _openIddictApplicationManager.FindByClientIdAsync("Default Application");
                app = existingApp as OpenIddictEntityFrameworkCoreApplication;
                var adminUser = _identityDbContext.Users.FirstOrDefault(t => t.UserName == "admin");
                if (adminUser != null) {
                    await _openIddictAuthorizationManager.CreateAsync(new OpenIddictAuthorizationDescriptor {
                        ApplicationId = app.Id,
                        CreationDate = DateTime.Now,
                        Status = "valid",
                        Subject = adminUser.UserId.ToString(),
                        Scopes = {
                            "profile",
                            "roles",
                            "openid"
                         },
                        Type = AuthorizationTypes.Permanent
                    });
                }
            }
        }
    }
}
