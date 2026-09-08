namespace FEAR.Domain.Model.Authentication
{
    /// <summary>
    /// Base class providing a user identifier property for authentication-related entities.
    /// </summary>
    public class UserIdObject
    {
        /// <summary>
        /// Gets or sets the unique identifier of the user associated with this object.
        /// </summary>
        public virtual Guid UserId { get; set; }
    }
}
