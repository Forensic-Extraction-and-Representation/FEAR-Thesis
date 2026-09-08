using FEAR.Domain.Dto.Authentication;
using FEAR.Host.Domain.Api.Authentication;
using System.Security.Claims;

namespace FEAR.Host.Core.Identity
{
    /// <summary>
    /// Defines a contract for identity and authentication operations.
    /// Provides methods for user authentication, token refresh, and user information retrieval based on claims.
    /// </summary>
    public interface IIdentityProvider
    {
        bool CanSignIn(User user);

        /// <summary>
        /// Retrieves the <see cref="User"/> entity from the database based on the provided <see cref="UserInfo"/>.
        /// </summary>
        /// <param name="userInfo">The user info containing the user ID and claims.</param>
        /// <returns>The <see cref="User"/> entity if found; otherwise, null.</returns>
        User GetUserFromClaim(UserInfo[] userInfo);

        /// <summary>
        /// Extracts user information and permissions from a claims principal.
        /// </summary>
        /// <param name="user">The claims principal representing the authenticated user.</param>
        /// <returns>A <see cref="UserInfo"/> object with user details and permissions.</returns>
        UserInfo[] GetUserInfoFromClaim(ClaimsPrincipal user);

        /// <summary>
        /// Authenticates a user and issues a JWT and refresh token if successful.
        /// </summary>
        /// <param name="request">The login request containing username and password.</param>
        /// <returns>A <see cref="LoginResponse"/> with token and user info if successful.</returns>
        Task<LoginResponse> Login(LoginRequest request);

        /// <summary>
        /// Refreshes a JWT using a valid refresh token, issuing a new JWT and refresh token.
        /// </summary>
        /// <param name="request">The refresh request containing the refresh token.</param>
        /// <returns>A <see cref="RefreshResponse"/> with new token and expiration if successful.</returns>
        Task<RefreshResponse> Refresh(RefreshRequest request);
    }
}
