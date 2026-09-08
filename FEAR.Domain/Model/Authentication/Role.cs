namespace FEAR.Domain.Model.Authentication
{
    /// <summary>
    /// Represents a role within the system, grouping a set of permissions that define what actions users assigned this role can perform.
    /// </summary>
    public class Role
    {
        /// <summary>
        /// Gets or sets the unique identifier for this role.
        /// </summary>
        public virtual Guid RoleId { get; set; }

        /// <summary>
        /// Gets or sets the name of the role.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the collection of permission sets associated with this role.
        /// Each permission set defines approved and denied actions for users assigned this role.
        /// </summary>
        public ICollection<PermissionSet> Permissions { get; set; }
    }
}
