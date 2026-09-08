using FEAR.Host.Domain.Api.Authentication;
using System.Net.Http.Json;
using Microsoft.JSInterop;

namespace FEAR.WASM.Services.Authentication
{
    public class AuthService : IAuthService
    {
        private LoginResponse _internalLoginData = null;
        protected readonly HttpClient _httpClient;
        private readonly IJSRuntime _jsRuntime;


        private SemaphoreSlim _semaphoreSlim = new SemaphoreSlim(1, 1);

        public AuthService(HttpClient httpClient, IJSRuntime jSRuntime)
        {
            _httpClient = httpClient;
            _jsRuntime = jSRuntime;
        }

        public Func<Task<HttpClient>> GetHttpClientFunc()
        {
            return async () =>
            {
                await ConfigureAccessToken();
                return _httpClient;
            };
        }

        private async Task SetLoinData(LoginResponse repsonse)
        {
            _internalLoginData = repsonse;
            string szLoginData = System.Text.Json.JsonSerializer.Serialize(repsonse);
            await _jsRuntime.InvokeVoidAsync("localStorage.setItem", "authToken", szLoginData);
        }

        private async Task LoadLoginData()
        {
            if (_internalLoginData == null)
            {
                string szLoginData = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", "authToken");
                if (!string.IsNullOrEmpty(szLoginData))
                    _internalLoginData = System.Text.Json.JsonSerializer.Deserialize<LoginResponse>(szLoginData);
            }
        }

        private async Task<LoginResponse> ConfigureAccessToken(LoginResponse loginData = null, bool attemptRefresh = true)
        {
            try
            {
                if (!string.IsNullOrEmpty(loginData?.Token))
                {
                    await SetLoinData(loginData);
                }
                else
                {
                    await LoadLoginData();
                }

                if (_internalLoginData?.Expires > DateTime.UtcNow)
                {
                    _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("bearer", _internalLoginData.Token);
                }
                else
                {
                    if (!string.IsNullOrEmpty(_internalLoginData?.RefreshToken) && attemptRefresh)
                    {
                        _internalLoginData = await RefreshToken();
                    }
                    else
                        _internalLoginData = null;

                    await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", "authToken");
                }

                return _internalLoginData;
            }
            catch (Exception ex)
            {
                _internalLoginData = null;
                _httpClient.DefaultRequestHeaders.Authorization = null;
                return new LoginResponse();
            }
        }

        private async Task CheckToken()
        {
            if (await ConfigureAccessToken(_internalLoginData) == null)
                await Logout();
        }

        public async Task<UserInfo> CurrentUserInfo()
        {
            await _semaphoreSlim.WaitAsync();
            await CheckToken();
            _semaphoreSlim.Release();

            if (_internalLoginData == null) return new UserInfo();

            var result = await _httpClient.GetFromJsonAsync<UserInfo>("v1.0/authentication/currentuserinfo");

            return result;
        }
        private async Task<LoginResponse> RefreshToken()
        {
            var result = await _httpClient.PostAsJsonAsync("v1.0/authentication/refreshtoken", new RefreshRequest() { RefreshToken = _internalLoginData.RefreshToken });
            LoginResponse resp = null;
            if (result.IsSuccessStatusCode)
            {
                resp = await result.Content.ReadFromJsonAsync<LoginResponse>();
                await ConfigureAccessToken(resp);
            }
            else
            {
                throw new Exception(await result.Content.ReadAsStringAsync());
            }

            return resp;
        }

        public async Task<LoginResponse> Login(LoginRequest loginRequest)
        {
            await _semaphoreSlim.WaitAsync();
            var result = await _httpClient.PostAsJsonAsync("v1.0/authentication/createtoken", loginRequest);
            LoginResponse resp = null;
            if (result.IsSuccessStatusCode)
            {
                resp = await result.Content.ReadFromJsonAsync<LoginResponse>();
                await ConfigureAccessToken(resp);
            }
            else
            {
                throw new Exception(await result.Content.ReadAsStringAsync());
            }

            _semaphoreSlim.Release();

            return resp;
        }

        public async Task Logout()
        {
            if (_internalLoginData == null) return;

            await _semaphoreSlim.WaitAsync();
            var result = await _httpClient.PostAsync("v1.0/authentication/logout", null);
            _semaphoreSlim.Release();
            result.EnsureSuccessStatusCode();
        }
    }
}
