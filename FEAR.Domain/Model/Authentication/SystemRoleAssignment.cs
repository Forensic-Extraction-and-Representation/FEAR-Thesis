namespace FEAR.Domain.Model.Authentication
{
    /// <summary>
    /// Represents a system-wide assignment of a role to a user.
    /// Inherits user and role identifiers from <see cref="AssignableRole"/>.
    /// </summary>
    public class SystemRoleAssignment : AssignableRole
    {
        /// <summary>
        /// Gets or sets the unique identifier for this system role assignment.
        /// </summary>
        public Guid SystemRoleAssignmentId { get; set; }
    }
}
