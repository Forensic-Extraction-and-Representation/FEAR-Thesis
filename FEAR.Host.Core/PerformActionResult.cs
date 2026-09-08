namespace FEAR.Host.Core
{
    /// <summary>
    /// Represents the result of a permission check for performing an action.
    /// Contains the roles that approve or deny the action and provides a summary of the result.
    /// </summary>
    public class PerformActionResult
    {
        /// <summary>
        /// Gets or sets the collection of role names that approve the action.
        /// </summary>
        public IEnumerable<string> ApprovingRoles { get; set; }

        /// <summary>
        /// Gets or sets the collection of role names that deny the action.
        /// </summary>
        public IEnumerable<string> DenyingRoles { get; set; }

        /// <summary>
        /// Gets a value indicating whether the action is approved (at least one approving role and no denying roles).
        /// </summary>
        public bool IsApproved => ApprovingRoles.Count() > 0 && DenyingRoles.Count() == 0;

        /// <summary>
        /// Gets a comma-separated string of denying role names.
        /// </summary>
        public string DenyingRolesMessage => string.Join(",", DenyingRoles);

        /// <summary>
        /// Initializes a new instance of the <see cref="PerformActionResult"/> class with empty role lists.
        /// </summary>
        public PerformActionResult()
        {
            ApprovingRoles = new List<string>();
            DenyingRoles = new List<string>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PerformActionResult"/> class with the specified approving and denying roles.
        /// </summary>
        /// <param name="approvingRoles">The roles that approve the action.</param>
        /// <param name="denyingRoles">The roles that deny the action.</param>
        public PerformActionResult(IEnumerable<string> approvingRoles, IEnumerable<string> denyingRoles)
        {
            ApprovingRoles = approvingRoles;
            DenyingRoles = denyingRoles;
        }
    }
}
