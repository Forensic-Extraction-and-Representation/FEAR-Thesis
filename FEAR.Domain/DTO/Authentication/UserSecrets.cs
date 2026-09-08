using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FEAR.Domain.Dto.Authentication
{
    /// <summary>
    /// Represents a user's secret information, such as encrypted credentials, associated with a user in the system.
    /// </summary>
    [Table("UserSecrets")]
    public class UserSecret : Model.Authentication.UserSecret
    {
        /// <summary>
        /// Gets or sets the unique identifier for the user associated with this secret.
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public override Guid UserId { get; set; }

        /// <summary>
        /// Gets or sets the user entity to which this secret belongs.
        /// </summary>
        public User User { get; set; }
    }
}
