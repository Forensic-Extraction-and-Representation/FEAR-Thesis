using FEAR.Blazor.Shared.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication.Internal;

namespace FEAR.WASM.Services
{
    public class WASMInternalHttpFactory : HttpFactory
    {
        public WASMInternalHttpFactory(IHttpClientFactory httpClientFactory, IAccessTokenProviderAccessor accessTokenProvider, NavigationManager navigationManager) : base(httpClientFactory, accessTokenProvider, navigationManager)
        {
        }

    }
}
