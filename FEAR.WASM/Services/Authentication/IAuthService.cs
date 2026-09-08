using FEAR.Host.Domain.Api.Authentication;

namespace FEAR.WASM.Services.Authentication
{
    public interface IAuthService
    {
        Task<UserInfo> CurrentUserInfo();
        Func<Task<HttpClient>> GetHttpClientFunc();
    }
}
