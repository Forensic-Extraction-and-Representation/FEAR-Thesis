namespace FEAR.Host.Core.Api.Management.Security
{
    /// <summary>
    /// Represents a request to update a user's password.
    /// Contains the user identifier, the current password, and the new password.
    /// </summary>
    public class UpdatePasswordRequest
    {
        /// <summary>
        /// The unique identifier of the user whose password is being updated.
        /// </summary>
        public Guid UserId { get; set; }

        /// <summary>
        /// The user's current password for verification.
        /// </summary>
        public string OldPassword { get; set; } = "";

        /// <summary>
        /// The new password to set for the user.
        /// </summary>
        public string NewPassword { get; set; }
    }

    /// <summary>
    /// Represents the response after attempting to update a user's password.
    /// </summary>
    public class UpdatePasswordResponse { }
}
