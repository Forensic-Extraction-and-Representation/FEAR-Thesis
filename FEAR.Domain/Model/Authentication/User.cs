namespace FEAR.Domain.Model.Authentication
{
    /// <summary>
    /// Represents a user within the system, including basic identity and creation information.
    /// Inherits the user identifier from <see cref="UserIdObject"/>.
    /// </summary>
    public class User : UserIdObject
    {
        /// <summary>
        /// Gets or sets the username associated with this user.
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// Gets or sets the email address of the user.
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// Gets or sets the date and time when the user was created.
        /// </summary>
        public DateTime CreatedDateTime { get; set; }
    }
}
