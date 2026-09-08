using FEAR.Domain.Dto.Authentication;
using FEAR.Domain.Helpers;
using FEAR.Host.Core.Identity;
using FEAR.Host.Core.IdentitySeed;
using FEAR.Host.Core.ProtectedEncryptionKey;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace FEAR.Host.Core.Database
{
    /// <summary>
    /// Provides functionality to seed the identity database with initial users, roles, permissions, and actions.
    /// Handles creation of admin user, system roles, and permission sets for a new or reset database.
    /// </summary>
    public class IdentityDbSeedProvider
    {
        private IConfiguration _configuration;
        private IPasswordProvider _passwordProvider;
        private IProtectedEncryptionKeyProvider _protectedEncryptionKeyProvider;
        private IdentityDbContext _identityDbContext;
        private PermissionSets _permissionSets = new PermissionSets();
        private SystemActionSets _systemActionSets = new SystemActionSets();

        /// <summary>
        /// Initializes a new instance of the <see cref="IdentityDbSeedProvider"/> class.
        /// </summary>
        /// <param name="configuration">Application configuration for seed options and encryption key names.</param>
        /// <param name="passwordProvider">Service for password hashing and validation.</param>
        /// <param name="protectedEncryptionKeyProvider">Service for encryption key management.</param>
        public IdentityDbSeedProvider(IConfiguration configuration, IPasswordProvider passwordProvider, IProtectedEncryptionKeyProvider protectedEncryptionKeyProvider)
        {
            _configuration = configuration;
            _passwordProvider = passwordProvider;
            _protectedEncryptionKeyProvider = protectedEncryptionKeyProvider;
        }

        /// <summary>
        /// Seeds the identity database with initial data, including admin user, roles, and permissions.
        /// Optionally deletes and recreates the database if allowed and configured.
        /// </summary>
        /// <param name="identityDbContext">The identity database context to seed.</param>
        /// <param name="allowDelete">If true, allows deletion and recreation of the database.</param>
        public void Seed(IdentityDbContext identityDbContext, bool allowDelete)
        {
            // Optionally delete the database if configured to reload seed data
            if (_configuration["DatabaseConfigurationOptions:ReloadSeedData"].ToLower() == "true")
            {
                if (allowDelete)
                    identityDbContext.Database.EnsureDeleted();
            }

            identityDbContext.Database.Migrate();
            _identityDbContext = identityDbContext;
            SeedData();
        }

        /// <summary>
        /// Seeds the database with system actions, permission sets, roles, and the default admin user.
        /// </summary>
        private void SeedData()
        {
            var existingUsers = _identityDbContext.Users.Any();

            if (!existingUsers)
            {
                using (var transaction = _identityDbContext.Database.BeginTransaction())
                {
                    // Add all system actions
                    _identityDbContext.SystemActions.AddRange(_systemActionSets.Actions);

                    // Add permission sets
                    var systemAdminPermissionSet = _permissionSets.SystemAdminPermissionSet();
                    var investigationManagementPermissionSet = _permissionSets.InvestigationAdminPermissionSet();
                    _identityDbContext.PermissionSets.Add(systemAdminPermissionSet);

                    // Create roles and assign permission sets
                    var systemAdminRole = new Role
                    {
                        Name = "SystemAdmin",
                        Permissions = new List<PermissionSet> { systemAdminPermissionSet }
                    };

                    var investigationManagementRole = new Role
                    {
                        Name = "InvestigationAdmin",
                        Permissions = new List<PermissionSet> { investigationManagementPermissionSet }
                    };

                    // Create the default admin user and assign the system admin role
                    var adminUser = CreateUser("admin", "admin", "admin@localdomain.local");
                    adminUser.SystemRoleAssignments = new List<SystemRoleAssignment> {
                    new SystemRoleAssignment
                    {
                        Role = systemAdminRole
                    }
                };

                    _identityDbContext.Users.Add(adminUser);
                    _identityDbContext.SaveChanges();
                    transaction.Commit();
                }
            }
        }

        /// <summary>
        /// Creates a new user with the specified username, password, and email.
        /// Handles password hashing, salt generation, and encryption of user secret data.
        /// </summary>
        /// <param name="username">The username for the new user.</param>
        /// <param name="password">The password for the new user.</param>
        /// <param name="email">The email address for the new user.</param>
        /// <returns>The created <see cref="User"/> object.</returns>
        private User CreateUser(string username, string password, string email)
        {
            var newUser = new User
            {
                UserId = Guid.NewGuid(),
                UserName = username,
                Email = email,
                CreatedDateTime = DateTime.UtcNow,
            };

            // Generate password salt and hash
            string passwordSalt = _passwordProvider.GenerateSalt();
            string defaultPassword = password;
            var passwordHash = _passwordProvider.HashPassword(defaultPassword, passwordSalt, 50000);

            var userSecretInfo = new FEAR.Domain.Model.Authentication.UserSecretInfo
            {
                PasswordHash = Convert.ToBase64String(passwordHash),
                Salt = passwordSalt,
                Iterations = 50000
            };

            var newUserSecret = new UserSecret
            {
                UserId = newUser.UserId,
                KeyName = _configuration["Authentication:EncryptionKeyName"]
            };

            newUser.UserSecret = newUserSecret;

            // Validate password (for sanity check)
            var valid = _passwordProvider.IsPasswordMatch(password, userSecretInfo);

            // Encrypt and store user secret data
            newUserSecret.SecretData = UserSecretHelpers.LockRecord(userSecretInfo, newUserSecret, _protectedEncryptionKeyProvider.EncryptData);

            return newUser;
        }
    }
}
