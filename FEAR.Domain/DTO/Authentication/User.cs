using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using VDS.RDF.Parsing;

namespace FEAR.Domain.Dto.Authentication
{
    /// <summary>
    /// Represents a user within the system, including their role assignments and authentication secrets.
    /// </summary>
    [Table("Users")]
    public class User : Model.Authentication.User
    {
        /// <summary>
        /// Gets or sets the unique identifier for this user.
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public override Guid UserId { get; set; }

        /// <summary>
        /// Gets or sets the collection of system-wide role assignments for this user.
        /// Each assignment links the user to a role that grants permissions across the entire system.
        /// </summary>
        public ICollection<SystemRoleAssignment> SystemRoleAssignments { get; set; }

        /// <summary>
        /// Gets or sets the collection of investigation-specific role assignments for this user.
        /// Each assignment links the user to a role within a particular investigation context.
        /// </summary>
        public ICollection<InvestigationRoleAssignment> InvestigationRoleAssignments { get; set; }

        /// <summary>
        /// Gets or sets the authentication secret associated with this user, such as credentials or keys.
        /// </summary>
        public UserSecret UserSecret { get; set; }

        public IEnumerable<String> GetRoles()
        {
            // These will need to be "INV:<ROLE>:ID" or "SYS:<ROLE>"
            foreach (var roleAssignment in SystemRoleAssignments)
            {
                yield return $"SYS:{roleAssignment.Role.Name}";
            }

            foreach (var roleAssignment in InvestigationRoleAssignments)
            {
                yield return $"INV:{roleAssignment.InvestigationId}:{roleAssignment.Role.Name}";
            }
        }
    }
}
