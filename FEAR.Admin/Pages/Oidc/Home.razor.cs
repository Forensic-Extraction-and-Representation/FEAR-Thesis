using FEAR.Admin.Services.Management;
using FEAR.Host.Domain.Api.Administration;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using OpenIddict.Abstractions;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace FEAR.Admin.Pages.Oidc
{
    [Authorize]
    public partial class Home
    {
        [Inject] OidcService OidcService { get; set; }

        protected override Func<OpenIdApplicationModel, bool> QuickFilters => x =>
        {
            if (string.IsNullOrWhiteSpace(_searchString))
                return true;

            if (x.DisplayName?.Contains(_searchString, StringComparison.OrdinalIgnoreCase) ?? false)
                return true;

            if (x.ClientId.Contains(_searchString, StringComparison.OrdinalIgnoreCase))
                return true;

            return false;
        };

        protected override async Task OnInitializedAsync()
        {
            // Subscribe to the event that triggers the modal display
            LoadOidcApplications();
        }

        private async void LoadOidcApplications()
        {
            Items = await OidcService.GetApplications();
            StateHasChanged();
        }

        private void EditOidcApp(OpenIdApplicationModel item)
        {
            editorDialog.BeginEdit(item);
        }

        private void DeleteOidcApp(OpenIdApplicationModel item)
        {
        }

        private void CreateOidcApp()
        {
            var newApp = new OpenIdApplicationModel();
            EditOidcApp(newApp);
        }

        private async Task SaveOidcApp(OpenIdApplicationModel updatedValue)
        {
            await OidcService.SaveApplication(updatedValue);
        }

        private void AddRequestUri(OpenIdApplicationModel app, string item)
        {
            app.RedirectUris.Add(new Uri(item));
        }

        private void DeleteRequestUri(OpenIdApplicationModel app, string uri)
        {
            app.RedirectUris.RemoveWhere(t => t.AbsoluteUri == uri);
        }

        private void TogglePermission(OpenIdApplicationModel openIdApp, string permission, bool isChecked)
        {
            if (isChecked)
            {
                if (!openIdApp.Permissions.Contains(permission))
                {
                    openIdApp.Permissions.Add(permission);
                }
            }
            else
            {
                if (openIdApp.Permissions.Contains(permission))
                {
                    openIdApp.Permissions.Remove(permission);
                }
            }
        }

        static List<string> ApplicationTypes = new()
        {
            OpenIddictConstants.ApplicationTypes.Native,
            OpenIddictConstants.ApplicationTypes.Web
        };

        static List<string> EndpointsOptions = new()
        {
            Permissions.Endpoints.Authorization,
            Permissions.Endpoints.DeviceAuthorization,
            Permissions.Endpoints.EndSession,
            Permissions.Endpoints.Introspection,
            Permissions.Endpoints.PushedAuthorization,
            Permissions.Endpoints.Revocation,
            Permissions.Endpoints.Token
        };

        static List<string> GrantTypesOptions = new()
        {
            Permissions.GrantTypes.AuthorizationCode,
            Permissions.GrantTypes.ClientCredentials,
            Permissions.GrantTypes.DeviceCode,
            Permissions.GrantTypes.Implicit,
            Permissions.GrantTypes.Password,
            Permissions.GrantTypes.RefreshToken,
            Permissions.GrantTypes.TokenExchange
        };

        static List<string> PrefixOptions = new()
        {
            Permissions.Prefixes.Audience,
            Permissions.Prefixes.Endpoint,
            Permissions.Prefixes.GrantType,
            Permissions.Prefixes.ResponseType,
            Permissions.Prefixes.Resource,
            Permissions.Prefixes.Scope
        };

        static List<string> ResponseTypesOptions = new List<string>()
        {
            Permissions.ResponseTypes.Code,
            Permissions.ResponseTypes.CodeIdToken,
            Permissions.ResponseTypes.CodeIdTokenToken,
            Permissions.ResponseTypes.CodeToken,
            Permissions.ResponseTypes.IdToken,
            Permissions.ResponseTypes.IdTokenToken,
            Permissions.ResponseTypes.None,
            Permissions.ResponseTypes.Token
        };  

        static List<string> ScopesOptions = new()
        {
            Permissions.Scopes.Address,
            Permissions.Scopes.Email,
            Permissions.Scopes.Phone,
            Permissions.Scopes.Profile,
            Permissions.Scopes.Roles
        };

        Dictionary<string, List<string>> permissions = new()
        {
            {"Endpoints", EndpointsOptions },
            {"Grant Types", GrantTypesOptions },
            {"Response Types", ResponseTypesOptions },
            {"Prefixes", PrefixOptions },
            {"Scopes", ScopesOptions  }
        };
    }
}