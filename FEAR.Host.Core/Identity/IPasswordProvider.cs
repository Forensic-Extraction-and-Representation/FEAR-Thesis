using FEAR.Domain.Dto.Authentication;
using Microsoft.AspNetCore.Identity;

namespace FEAR.Host.Core.Identity
{
    /// <summary>
    /// Defines a contract for password hashing, verification, and salt generation for user authentication.
    /// Extends <see cref="IPasswordHasher{User}"/> to provide additional methods for secure password management.
    /// </summary>
    public interface IPasswordProvider : IPasswordHasher<User>
    {
        /// <summary>
        /// Hashes a password using the specified salt and iteration count.
        /// </summary>
        /// <param name="password">The plain text password to hash.</param>
        /// <param name="salt">The cryptographic salt to use in the hash.</param>
        /// <param name="iterations">The number of hash iterations to perform.</param>
        /// <returns>A byte array containing the hashed password.</returns>
        byte[] HashPassword(string password, string salt, int iterations);

        /// <summary>
        /// Verifies whether the provided password matches the stored hash and salt in the given <see cref="FEAR.Domain.Models.Authentication.UserSecretInfo"/>.
        /// </summary>
        /// <param name="password">The plain text password to verify.</param>
        /// <param name="usi">The user's secret info containing the hash, salt, and iteration count.</param>
        /// <returns>True if the password matches; otherwise, false.</returns>
        bool IsPasswordMatch(string password, FEAR.Domain.Model.Authentication.UserSecretInfo usi);

        /// <summary>
        /// Generates a new cryptographically secure salt for password hashing.
        /// </summary>
        /// <returns>A string representing the generated salt.</returns>
        string GenerateSalt();
    }
}
