using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FEAR.Domain.Dto.Authentication
{
    /// <summary>
    /// Represents a role within the system, including its permissions and assignments.
    /// Roles can include system-wide roles and investigation-specific roles.
    /// </summary>
    [Table("Roles")]
    public class Role : Model.Authentication.Role
    {
        /// <summary>
        /// Gets or sets the unique identifier for this role.
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public override Guid RoleId { get; set; }

        /// <summary>
        /// Gets or sets the collection of permission sets associated with this role.
        /// These define the actions that users assigned this role are allowed or denied to perform.
        /// </summary>
        public new ICollection<PermissionSet> Permissions { get; set; }

        /// <summary>
        /// Gets or sets the collection of investigation-specific role assignments for this role.
        /// Each assignment links a user to this role within a particular investigation context.
        /// </summary>
        public ICollection<InvestigationRoleAssignment> InvestigationRoleAssignments { get; set; }

        /// <summary>
        /// Gets or sets the collection of system-wide role assignments for this role.
        /// Each assignment links a user to this role at the system level.
        /// </summary>
        public ICollection<SystemRoleAssignment> SystemRoleAssignments { get; set; }
    }
}
