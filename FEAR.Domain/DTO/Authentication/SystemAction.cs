using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FEAR.Domain.Dto.Authentication
{
    /// <summary>
    /// Represents an action that can be performed within the system, used for permission and authorization checks.
    /// </summary>
    [Table("SystemActions")]
    public class SystemAction : Model.Authentication.SystemAction
    {
        /// <summary>
        /// Gets or sets the unique identifier for this system action.
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public override Guid SystemActionId { get; set; }

        /// <summary>
        /// Gets or sets the collection of permission sets that explicitly approve this action.
        /// Users or roles assigned these permission sets are allowed to perform this action.
        /// </summary>
        public ICollection<PermissionSet> ApprovedPermissionSets { get; set; }

        /// <summary>
        /// Gets or sets the collection of permission sets that explicitly deny this action.
        /// Users or roles assigned these permission sets are not allowed to perform this action, even if approved elsewhere.
        /// </summary>
        public ICollection<PermissionSet> DeniedPermissionSets { get; set; }
    }
}
