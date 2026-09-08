namespace FEAR.Domain.Model.Authentication
{
    /// <summary>
    /// Represents a role that can be assigned to a user.
    /// Inherits the user identifier from <see cref="UserIdObject"/>.
    /// </summary>
    public class AssignableRole : UserIdObject
    {
        /// <summary>
        /// Gets or sets the unique identifier of the assigned role.
        /// </summary>
        public Guid RoleId { get; set; }
    }
}
