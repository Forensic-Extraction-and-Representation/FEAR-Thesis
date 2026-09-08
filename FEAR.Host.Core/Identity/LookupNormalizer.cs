using Microsoft.AspNetCore.Identity;

namespace FEAR.Host.Core.Identity
{
    /// <summary>
    /// Provides identity and authentication services, including login, token refresh, and user info retrieval.
    /// Handles JWT token generation, refresh token management, and user claims extraction.
    /// </summary>
    public class LookupNormalizer : ILookupNormalizer
    {
        /// <summary>
        /// Normalizes a user name to upper invariant for consistent lookups.
        /// </summary>
        public string NormalizeName(string name)
        {
            return name.ToUpperInvariant();
        }
        /// <summary>
        /// Normalizes an email address to upper invariant for consistent lookups.
        /// </summary>
        public string NormalizeEmail(string email)
        {
            return email.ToUpperInvariant();
        }
    }
}
