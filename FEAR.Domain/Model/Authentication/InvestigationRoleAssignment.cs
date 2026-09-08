namespace FEAR.Domain.Model.Authentication
{
    /// <summary>
    /// Represents the assignment of a role to a user within a specific investigation.
    /// </summary>
    public class InvestigationRoleAssignment : AssignableRole
    {
        /// <summary>
        /// Gets or sets the unique identifier for this investigation role assignment.
        /// </summary>
        public Guid InvestigationRoleAssignmentId { get; set; }

        /// <summary>
        /// Gets or sets the unique identifier of the investigation to which this role assignment applies.
        /// </summary>
        public Guid InvestigationId { get; set; }
    }
}
