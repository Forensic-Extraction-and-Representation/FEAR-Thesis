using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FEAR.Domain.Dto.Authentication
{
    /// <summary>
    /// Represents a set of permissions, including approved and denied actions, and the roles associated with this set.
    /// </summary>
    [Table("PermissionSets")]
    public class PermissionSet : Model.Authentication.PermissionSet
    {
        /// <summary>
        /// Gets or sets the unique identifier for this permission set.
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public override Guid PermissionSetId { get; set; }

        /// <summary>
        /// Gets or sets the collection of system actions that are explicitly approved by this permission set.
        /// Users or roles assigned this set are allowed to perform these actions.
        /// </summary>
        public new ICollection<SystemAction> ApprovedActions { get; set; }

        /// <summary>
        /// Gets or sets the collection of system actions that are explicitly denied by this permission set.
        /// Users or roles assigned this set are not allowed to perform these actions, even if approved elsewhere.
        /// </summary>
        public new ICollection<SystemAction> DeniedActions { get; set; }

        /// <summary>
        /// Gets or sets the collection of roles that this permission set is associated with.
        /// These roles inherit the permissions defined by this set.
        /// </summary>
        public virtual ICollection<Role> Roles { get; set; }
    }
}
