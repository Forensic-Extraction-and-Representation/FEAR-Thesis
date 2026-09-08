namespace FEAR.Domain.Model.Authentication
{
    /// <summary>
    /// Represents a set of permissions, including collections of approved and denied actions.
    /// Used to control access and authorization for users or roles within the system.
    /// </summary>
    public class PermissionSet
    {
        /// <summary>
        /// Gets or sets the unique identifier for this permission set.
        /// </summary>
        public virtual Guid PermissionSetId { get; set; }

        /// <summary>
        /// Gets or sets the name of the permission set.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the collection of system actions that are explicitly approved by this permission set.
        /// Users or roles assigned this set are allowed to perform these actions.
        /// </summary>
        public virtual ICollection<SystemAction> ApprovedActions { get; set; }

        /// <summary>
        /// Gets or sets the collection of system actions that are explicitly denied by this permission set.
        /// Users or roles assigned this set are not allowed to perform these actions, even if approved elsewhere.
        /// </summary>
        public virtual ICollection<SystemAction> DeniedActions { get; set; }
    }
}
