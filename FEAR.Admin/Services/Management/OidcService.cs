using FEAR.Blazor.Shared.Services;
using FEAR.Host.Domain.Api.Administration;

namespace FEAR.Admin.Services.Management
{
    public class OidcService : BaseService
    {
        public OidcService(HttpFactory httpClientFactory) : base(httpClientFactory)
        {
        }

        public async Task<IEnumerable<OpenIdApplicationModel>> GetApplications()
        {
            return await GetAsync<IEnumerable<OpenIdApplicationModel>>("v1.0/Management/Oidc/GetApplications");
        }

        public async Task SaveApplication(OpenIdApplicationModel model)
        {
            await PostAsync("v1.0/Management/Oidc/SaveApplication", model);
        }
    }
}
