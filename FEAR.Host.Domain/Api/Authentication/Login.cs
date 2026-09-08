using System.Security.Claims;

namespace FEAR.Host.Domain.Api.Authentication
{
    /// <summary>
    /// Represents a request to authenticate a user.
    /// Contains the credentials and options required for login.
    /// </summary>
    public class LoginRequest
    {
        /// <summary>
        /// The username of the user attempting to log in.
        /// </summary>
        public string Username { get; set; }

        /// <summary>
        /// The password of the user attempting to log in.
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// Indicates whether the login session should be persistent.
        /// </summary>
        public bool RememberMe { get; set; }
    }

    /// <summary>
    /// Represents the response returned after a login attempt.
    /// Contains authentication tokens, user information, and status.
    /// </summary>
    public class LoginResponse
    {
        /// <summary>
        /// The JWT or authentication token issued upon successful login.
        /// </summary>
        public string Token { get; set; }

        /// <summary>
        /// The expiration date and time of the authentication token.
        /// </summary>
        public DateTime? Expires { get; set; }

        /// <summary>
        /// The refresh token that can be used to obtain a new authentication token.
        /// </summary>
        public string RefreshToken { get; set; }

        /// <summary>
        /// The unique identifier of the authenticated user.
        /// </summary>
        public Guid? UserId { get; set; }

        /// <summary>
        /// Indicates whether the login attempt was successful.
        /// </summary>
        public bool IsSuccess { get; set; }
        public ClaimsIdentity UserIdentity { get; set; }
    }
}
