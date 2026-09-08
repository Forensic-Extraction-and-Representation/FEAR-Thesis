using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FEAR.Domain.Dto.Authentication
{
    /// <summary>
    /// Represents a system-wide assignment of a role to a user.
    /// Used to associate users with roles that grant permissions across the entire system.
    /// </summary>
    [Table("SystemRoleAssignments")]
    public class SystemRoleAssignment : Model.Authentication.SystemRoleAssignment
    {
        /// <summary>
        /// Gets or sets the unique identifier for this system role assignment.
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public new Guid SystemRoleAssignmentId { get; set; }

        /// <summary>
        /// Gets or sets the user to whom the system role is assigned.
        /// </summary>
        public User User { get; set; }

        /// <summary>
        /// Gets or sets the role assigned to the user at the system level.
        /// </summary>
        public Role Role { get; set; }
    }
}
