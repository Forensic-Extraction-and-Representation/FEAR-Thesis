using FEAR.Api.Controllers;
using FEAR.Host.Core;
using FEAR.Host.Core.Identity;
using FEAR.Host.Domain.Api.Administration;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OpenIddict.Abstractions;
using OpenIddict.Core;
using OpenIddict.EntityFrameworkCore.Models;
using OpenIddict.Validation.AspNetCore;
using System.Linq;
using VDS.Common.Collections.Enumerations;

namespace FEAR.Hosted.Controllers.Management
{
    [ApiController]
    [Route("v1.0/Management/[controller]/[action]")]
    public class OidcController : AuthenticatedControllerBase
    {
        protected OpenIdApplicationDbContext openIdApplicationDbContext;
        protected IOpenIddictApplicationManager openIddictApplicationManager;
        public OidcController(IIdentityProvider identityProvider, IPermissionProvider permissionProvider,
            OpenIdApplicationDbContext openIdApplicationDbContext, IOpenIddictApplicationManager openIddictApplicationManager)
            : base(identityProvider, permissionProvider)
        {
            this.openIdApplicationDbContext = openIdApplicationDbContext;
            this.openIddictApplicationManager = openIddictApplicationManager;
        }

        [Authorize(AuthenticationSchemes = $"{OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme}")]
        [SystemActionAuthorize("System.OpenIdConnect.ManageSettings")]
        public async Task<IActionResult> GetApplications()
        {
            List<OpenIddictApplicationDescriptor> apps = new List<OpenIddictApplicationDescriptor>();
            await foreach (var application in openIddictApplicationManager.ListAsync())
            {
                var d = new OpenIddictApplicationDescriptor();
                var r = openIddictApplicationManager.PopulateAsync(d, application);
                apps.Add(d);
            }

            return Ok(apps);
        }

        [Authorize(AuthenticationSchemes = $"{OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme}")]
        [SystemActionAuthorize("System.OpenIdConnect.ManageSettings")]
        public async Task<IActionResult> SaveApplication([FromBody] OpenIdApplicationModel model)
        {
            if (model == null || string.IsNullOrEmpty(model.ClientId))
                return BadRequest("Invalid application data.");
            var existingApp = await openIddictApplicationManager.FindByClientIdAsync(model.ClientId);
            if (existingApp != null)
            {
                // Map the properties from the model to the existing application
                var descriptor = new OpenIddictApplicationDescriptor();
                await openIddictApplicationManager.PopulateAsync(existingApp, descriptor);

                if (descriptor != null)
                {
                    descriptor.ApplicationType = model.ApplicationType;
                    descriptor.ClientId = model.ClientId;
                    descriptor.ClientSecret = model.ClientSecret;
                    descriptor.ClientType = model.ClientType;
                    descriptor.ConsentType = model.ConsentType;
                    descriptor.DisplayName = model.DisplayName;
                    descriptor.JsonWebKeySet = model.JsonWebKeySet;

                    descriptor.DisplayNames.Clear();
                    descriptor.Permissions.Clear();
                    descriptor.PostLogoutRedirectUris.Clear();
                    descriptor.RedirectUris.Clear();
                    descriptor.Requirements.Clear();

                    model.DisplayNames.ToList().ForEach(t => descriptor.DisplayNames.Add(t.Key, t.Value));
                    model.Permissions.ToList().ForEach(t => descriptor.Permissions.Add(t));
                    model.PostLogoutRedirectUris.ToList().ForEach(t => descriptor.PostLogoutRedirectUris.Add(t));
                    model.RedirectUris.ToList().ForEach(t => descriptor.RedirectUris.Add(t));
                    model.Requirements.ToList().ForEach(t => descriptor.Requirements.Add(t));
                }
                else
                {
                    return BadRequest("Application type mismatch.");
                }
                // Update existing application
                await openIddictApplicationManager.UpdateAsync(existingApp, descriptor);
            }
            else
            {
                var descriptor = new OpenIddictApplicationDescriptor
                {
                    ApplicationType = model.ApplicationType,
                    ClientId = model.ClientId,
                    ClientSecret = model.ClientSecret,
                    ClientType = model.ClientType,
                    ConsentType = model.ConsentType,
                    DisplayName = model.DisplayName,
                    JsonWebKeySet = model.JsonWebKeySet
                };

                descriptor.DisplayNames.Clear();
                descriptor.Permissions.Clear();
                descriptor.PostLogoutRedirectUris.Clear();
                descriptor.RedirectUris.Clear();
                descriptor.Requirements.Clear();

                model.DisplayNames.ToList().ForEach(t => descriptor.DisplayNames.Add(t.Key, t.Value));
                model.Permissions.ToList().ForEach(t => descriptor.Permissions.Add(t));
                model.PostLogoutRedirectUris.ToList().ForEach(t => descriptor.PostLogoutRedirectUris.Add(t));
                model.RedirectUris.ToList().ForEach(t => descriptor.RedirectUris.Add(t));
                model.Requirements.ToList().ForEach(t => descriptor.Requirements.Add(t));

                try
                {
                    // Create new application
                    await openIddictApplicationManager.CreateAsync(descriptor);
                }
                catch (Exception ex)
                {
                }
            }
            return Ok();
        }
    }
}
