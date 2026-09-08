namespace FEAR.Host.Core
{
    /// <summary>
    /// Represents a user session token for interacting with a case from Autopsy.
    /// Used to authenticate and authorize user actions on a specific case within the service.
    /// </summary>
    [Obsolete("This should ideally be obsolete given the introduction of th OIDC authentication system.")]
    public class UserCaseToken
    {
        /// <summary>
        /// The unique identifier of the user interacting with the case service.
        /// </summary>
        public string UserID { get; set; }

        /// <summary>
        /// The token used to authenticate the user's session with the service.
        /// </summary>
        public string Token { get; set; }
    }
}
