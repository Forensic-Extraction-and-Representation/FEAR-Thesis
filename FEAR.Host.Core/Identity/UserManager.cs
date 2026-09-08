using FEAR.Domain.Dto.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FEAR.Host.Core.Identity
{
    /// <summary>
    /// Application-specific user manager for handling user-related operations such as creation, validation, and authentication.
    /// Inherits from <see cref="UserManager{User}"/> and is configured for the FEAR authentication domain.
    /// </summary>
    public class UserManager : UserManager<User>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UserManager"/> class with the specified dependencies.
        /// </summary>
        /// <param name="store">The user store implementation for persisting user data.</param>
        /// <param name="optionsAccessor">The accessor for identity options.</param>
        /// <param name="passwordHasher">The password hasher for user passwords.</param>
        /// <param name="userValidators">A collection of user validators.</param>
        /// <param name="passwordValidators">A collection of password validators.</param>
        /// <param name="keyNormalizer">The normalizer for user lookup keys.</param>
        /// <param name="errors">The error describer for identity errors.</param>
        /// <param name="services">The service provider for dependency injection.</param>
        /// <param name="logger">The logger for user manager operations.</param>
        public UserManager(
            IUserStore<User> store,
            IOptions<IdentityOptions> optionsAccessor,
            IPasswordHasher<User> passwordHasher,
            IEnumerable<IUserValidator<User>> userValidators,
            IEnumerable<IPasswordValidator<User>> passwordValidators,
            ILookupNormalizer keyNormalizer,
            IdentityErrorDescriber errors,
            IServiceProvider services,
            ILogger<UserManager<User>> logger)
            : base(store, optionsAccessor, passwordHasher, userValidators, passwordValidators, keyNormalizer, errors, services, logger)
        {
        }
    }
}
