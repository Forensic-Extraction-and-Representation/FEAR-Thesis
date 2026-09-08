namespace FEAR.Domain.Model.Authentication
{
    /// <summary>
    /// Represents an action that can be performed within the system.
    /// Used for defining and managing permissions and authorization checks.
    /// </summary>
    public class SystemAction
    {
        /// <summary>
        /// Gets or sets the unique identifier for this system action.
        /// </summary>
        public virtual Guid SystemActionId { get; set; }

        /// <summary>
        /// Gets or sets the name of the system action.
        /// </summary>
        public string Name { get; set; }
    }
}
