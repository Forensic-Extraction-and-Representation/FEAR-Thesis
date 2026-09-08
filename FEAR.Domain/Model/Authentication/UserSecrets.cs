namespace FEAR.Domain.Model.Authentication
{
    /// <summary>
    /// Represents a secret associated with a user, such as a credential, key, or other sensitive information.
    /// Inherits the user identifier from <see cref="UserIdObject"/>.
    /// </summary>
    public class UserSecret : UserIdObject
    {
        /// <summary>
        /// Gets or sets the name of the secret that encrypts the SecretData.
        /// </summary>
        public string KeyName { get; set; }

        /// <summary>
        /// Gets or sets the secret data or value, such as the actual credential or key.
        /// </summary>
        public string SecretData { get; set; }
    }
}
