namespace FEAR.Host.Domain.Api.Authentication
{
    /// <summary>
    /// Represents a request to refresh an authentication token.
    /// Contains the refresh token issued during a previous authentication.
    /// </summary>
    public class RefreshRequest
    {
        /// <summary>
        /// The refresh token used to obtain a new authentication token.
        /// </summary>
        public string RefreshToken { get; set; }
    }

    /// <summary>
    /// Represents the response returned after a successful token refresh operation.
    /// Contains the new authentication token, its expiration, and a new refresh token.
    /// </summary>
    public class RefreshResponse
    {
        /// <summary>
        /// The new authentication token issued after refreshing.
        /// </summary>
        public string Token { get; set; }

        /// <summary>
        /// The expiration date and time of the new authentication token.
        /// </summary>
        public DateTime? Expires { get; set; }

        /// <summary>
        /// The new refresh token that can be used for future refresh operations.
        /// </summary>
        public string RefreshToken { get; set; }
    }
}
