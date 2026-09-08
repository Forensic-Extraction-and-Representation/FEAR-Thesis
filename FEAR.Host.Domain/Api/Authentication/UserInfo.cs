namespace FEAR.Host.Domain.Api.Authentication
{
    /// <summary>
    /// Represents user information and authentication state for the current session.
    /// Includes credentials, claims, and permissions for system and investigation actions.
    /// </summary>
    public class UserInfo
    {
        /// <summary>
        /// The username of the user.
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// The password of the user.
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// Indicates whether the user is authenticated.
        /// </summary>
        public bool IsAuthenticated { get; set; }

        /// <summary>
        /// A collection of claims associated with the user (e.g., roles, permissions).
        /// </summary>
        public IEnumerable<KeyValuePair<string, string>> Claims { get; set; }

        /// <summary>
        /// The unique identifier of the user.
        /// </summary>
        public Guid UserId { get; set; }

        /// <summary>
        /// The set of system-wide actions the user is permitted to perform.
        /// </summary>
        public ICollection<string> SystemActions { get; set; }

        /// <summary>
        /// The set of actions the user can perform for each investigation, keyed by investigation ID.
        /// </summary>
        public IDictionary<Guid, ICollection<string>> InvestigationActions { get; set; }
    }
}
