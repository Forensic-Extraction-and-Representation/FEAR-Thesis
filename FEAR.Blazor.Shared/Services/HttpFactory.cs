using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication.Internal;

namespace FEAR.Blazor.Shared.Services
{
    public class HttpFactory
    {
        private IHttpClientFactory _httpClientFactory;
        private IAccessTokenProviderAccessor _accessTokenProvider;
        private NavigationManager _navigationManager;

        public HttpFactory(IHttpClientFactory httpClientFactory, IAccessTokenProviderAccessor accessTokenProvider, NavigationManager navigationManager)
        {
            _httpClientFactory = httpClientFactory;
            _accessTokenProvider = accessTokenProvider;
            _navigationManager = navigationManager;
        }

        public async Task<HttpClient> CreateHttpClientAsync(bool withToken = true)
        {
            var client = _httpClientFactory.CreateClient("FEAR-HOSTED.API");
            if (withToken)
            {
                var tokenRequest = await _accessTokenProvider.TokenProvider.RequestAccessToken();
                if (tokenRequest.TryGetToken(out var token))
                {
                    client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token.Value);
                }
                else
                {
                    _navigationManager.NavigateToLogin(
                        $"authentication/login?ReturnUrl={Uri.EscapeDataString(_navigationManager.Uri)}",
                        new InteractiveRequestOptions { Interaction = InteractionType.SignIn, ReturnUrl = _navigationManager.Uri });
                    throw new UnauthorizedAccessException("Access token is not available.");
                }
            }

            return client;
        }

    }
}
