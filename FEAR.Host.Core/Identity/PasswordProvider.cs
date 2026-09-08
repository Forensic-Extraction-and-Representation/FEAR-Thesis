using FEAR.Domain.Dto.Authentication;
using FEAR.Domain.Helpers;
using FEAR.Host.Core.ProtectedEncryptionKey;
using Microsoft.AspNetCore.Identity;
using System.Security.Cryptography;

namespace FEAR.Host.Core.Identity
{
    /// <summary>
    /// Provides password hashing, verification, and salt generation for user authentication.
    /// Implements <see cref="IPasswordProvider"/> for secure password management using PBKDF2 (Rfc2898DeriveBytes) and SHA-256.
    /// </summary>
    public class PasswordProvider : IPasswordProvider
    {
        private readonly IProtectedEncryptionKeyProvider _protectedEncryptionKeyProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="PasswordProvider"/> class.
        /// </summary>
        /// <param name="protectedEncryptionKeyProvider">The encryption key provider for unlocking user secrets.</param>
        public PasswordProvider(IProtectedEncryptionKeyProvider protectedEncryptionKeyProvider)
        {
            _protectedEncryptionKeyProvider = protectedEncryptionKeyProvider;
        }

        /// <summary>
        /// Hashes a password using PBKDF2 (Rfc2898DeriveBytes) with the specified salt and iteration count.
        /// </summary>
        /// <param name="password">The plain text password to hash.</param>
        /// <param name="salt">The cryptographic salt (Base64 encoded).</param>
        /// <param name="iterations">The number of hash iterations to perform.</param>
        /// <returns>A byte array containing the hashed password.</returns>
        public byte[] HashPassword(string password, string salt, int iterations)
        {
            return new Rfc2898DeriveBytes(password, Convert.FromBase64String(salt), iterations, HashAlgorithmName.SHA256).GetBytes(256);
        }

        /// <summary>
        /// Verifies whether the provided password matches the stored hash and salt in the given <see cref="FEAR.Domain.Models.Authentication.UserSecretInfo"/>.
        /// </summary>
        /// <param name="password">The plain text password to verify.</param>
        /// <param name="usi">The user's secret info containing the hash, salt, and iteration count.</param>
        /// <returns>True if the password matches; otherwise, false.</returns>
        public bool IsPasswordMatch(string password, FEAR.Domain.Model.Authentication.UserSecretInfo usi)
        {
            byte[] rfc2898DeriveBytes = HashPassword(password, usi.Salt, usi.Iterations);
            return Convert.ToBase64String(rfc2898DeriveBytes) == usi.PasswordHash;
        }

        /// <summary>
        /// Generates a new cryptographically secure salt for password hashing.
        /// </summary>
        /// <returns>A string representing the generated salt (Base64 encoded).</returns>
        public string GenerateSalt()
        {
            byte[] array = new byte[32];
            using (RNGCryptoServiceProvider rNGCryptoServiceProvider = new RNGCryptoServiceProvider())
            {
                rNGCryptoServiceProvider.GetBytes(array);
                return Convert.ToBase64String(array);
            }
        }

        /// <summary>
        /// Hashes a password for a user and updates the user's secret with the new hash, salt, and iteration count.
        /// </summary>
        /// <param name="user">The user whose password is being hashed.</param>
        /// <param name="password">The plain text password to hash.</param>
        /// <returns>The Base64-encoded password hash.</returns>
        public string HashPassword(User user, string password)
        {
            if (user.UserSecret == null)
            {
                throw new ArgumentNullException(nameof(user.UserSecret));
            }

            FEAR.Domain.Model.Authentication.UserSecretInfo usi = UserSecretHelpers.UnlockRecord(user.UserSecret, _protectedEncryptionKeyProvider.DecryptData);

            string salt = GenerateSalt();
            byte[] rfc2898DeriveBytes = HashPassword(password, salt, usi.Iterations);
            usi.PasswordHash = Convert.ToBase64String(rfc2898DeriveBytes);
            usi.Salt = salt;
            usi.Iterations = 50000;
            user.UserSecret.SecretData = UserSecretHelpers.LockRecord(usi, user.UserSecret, _protectedEncryptionKeyProvider.EncryptData);
            return usi.PasswordHash;
        }

        /// <summary>
        /// Verifies a hashed password against a provided password for a user.
        /// </summary>
        /// <param name="user">The user whose password is being verified.</param>
        /// <param name="hashedPassword">The stored hashed password (Base64 encoded).</param>
        /// <param name="providedPassword">The plain text password to verify.</param>
        /// <returns>A <see cref="PasswordVerificationResult"/> indicating success or failure.</returns>
        public PasswordVerificationResult VerifyHashedPassword(User user, string hashedPassword, string providedPassword)
        {
            if (user.UserSecret == null)
            {
                throw new ArgumentNullException(nameof(user.UserSecret));
            }

            FEAR.Domain.Model.Authentication.UserSecretInfo usi = UserSecretHelpers.UnlockRecord(user.UserSecret, _protectedEncryptionKeyProvider.DecryptData);

            if (IsPasswordMatch(providedPassword, usi))
            {
                return PasswordVerificationResult.Success;
            }
            else
            {
                return PasswordVerificationResult.Failed;
            }
        }
    }
}
