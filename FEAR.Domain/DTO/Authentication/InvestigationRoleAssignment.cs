using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FEAR.Domain.Dto.Authentication
{
    /// <summary>
    /// Represents an assignment of a role to a user within a specific investigation context.
    /// </summary>
    [Table("InvestigationRoleAssignments")]
    public class InvestigationRoleAssignment : Model.Authentication.InvestigationRoleAssignment
    {
        /// <summary>
        /// Gets or sets the unique identifier for this investigation role assignment.
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public new Guid InvestigationRoleAssignmentId { get; set; }

        /// <summary>
        /// Gets or sets the user to whom the role is assigned for the investigation.
        /// </summary>
        public User User { get; set; }

        /// <summary>
        /// Gets or sets the role assigned to the user for the investigation.
        /// </summary>
        public Role Role { get; set; }
    }
}
