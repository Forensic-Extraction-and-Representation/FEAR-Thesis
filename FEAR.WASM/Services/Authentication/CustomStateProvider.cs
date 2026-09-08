using FEAR.Host.Domain.Api.Authentication;
using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;

namespace FEAR.WASM.Services.Authentication
{
    public class CustomStateProvider : AuthenticationStateProvider
    {
        private readonly IAuthService _authService;
        private UserInfo _currentUser;
        public CustomStateProvider(IAuthService authService)
        {
            _authService = authService;
        }

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            var identity = new ClaimsIdentity();
            try
            {
                var userInfo = await GetUserInfo();
                if (userInfo.IsAuthenticated)
                {
                    var claims = new[] { new Claim(ClaimTypes.Name, _currentUser.UserName) }.Concat(_currentUser.Claims.Select(c => new Claim(c.Key, c.Value)));
                    identity = new ClaimsIdentity(claims, "Server authentication");
                }
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine("Request failed:" + ex.ToString());
            }

            return new AuthenticationState(new ClaimsPrincipal(identity));
        }

        public async Task<UserInfo> GetUserInfo()
        {
            if (_currentUser != null && _currentUser.IsAuthenticated) return _currentUser;
            _currentUser = await _authService.CurrentUserInfo();
            return _currentUser;
        }
    }
}
