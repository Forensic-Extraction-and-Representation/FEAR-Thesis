namespace FEAR.Host.Core.Api.Management.Security
{
    /// <summary>
    /// Represents a request to create a new user in the system.
    /// Contains the necessary user credentials and contact information.
    /// </summary>
    public class CreateUserRequest
    {
        /// <summary>
        /// The username for the new user.
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// The email address for the new user.
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// The password for the new user.
        /// </summary>
        public string Password { get; set; }
    }

    /// <summary>
    /// Represents the response returned after attempting to create a user.
    /// Contains the unique identifier of the user and the result status.
    /// </summary>
    public class CreateUserResponse
    {
        /// <summary>
        /// The unique identifier assigned to the newly created user.
        /// </summary>
        public Guid UserId { get; set; }

        /// <summary>
        /// Indicates whether the user creation was successful.
        /// </summary>
        public bool Success { get; set; }
    }
}
