namespace FEAR.Domain.Model.Authentication
{
    /// <summary>
    /// Represents a refresh token associated with a user, used to maintain authentication sessions and enable secure token renewal.
    /// Inherits the user identifier from <see cref="UserIdObject"/>.
    /// </summary>
    public class UserRefreshToken : UserIdObject
    {
        /// <summary>
        /// Gets or sets the unique identifier for this user refresh token.
        /// </summary>
        public Guid UserRefreshTokenId { get; set; }

        /// <summary>
        /// Gets or sets the refresh token string used for renewing authentication sessions.
        /// </summary>
        public string RefreshToken { get; set; }

        /// <summary>
        /// Gets or sets the expiration date and time for this refresh token.
        /// </summary>
        public DateTime ValidTo { get; set; }
    }
}
