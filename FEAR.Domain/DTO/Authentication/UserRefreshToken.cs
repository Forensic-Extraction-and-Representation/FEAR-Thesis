using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FEAR.Domain.Dto.Authentication
{
    /// <summary>
    /// Represents a refresh token associated with a user, used to maintain authentication sessions.
    /// </summary>
    [Table("UserRefreshTokens")]
    public class UserRefreshToken : Model.Authentication.UserRefreshToken
    {
        /// <summary>
        /// Gets or sets the unique identifier for this user refresh token.
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public new Guid UserRefreshTokenId { get; set; }
    }
}
