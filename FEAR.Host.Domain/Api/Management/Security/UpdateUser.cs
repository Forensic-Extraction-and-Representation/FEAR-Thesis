namespace FEAR.Host.Core.Api.Management.Security
{
    /// <summary>
    /// Represents a request to update an existing user's information.
    /// Inherits user credential and contact properties from <see cref="CreateUserRequest"/>,
    /// and adds the unique identifier of the user to update.
    /// </summary>
    public class UpdateUserRequest : CreateUserRequest
    {
        /// <summary>
        /// The unique identifier of the user to update.
        /// </summary>
        public Guid UserId { get; set; }
    }

    /// <summary>
    /// Represents the response after attempting to update a user's information.
    /// </summary>
    public class UpdateUserResponse { }
}
