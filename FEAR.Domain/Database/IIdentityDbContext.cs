using FEAR.Domain.Dto.Authentication;
using Microsoft.EntityFrameworkCore;

namespace FEAR.Domain.Database
{
    public interface IIdentityDbContext
    {
        /// <summary>
        /// Gets or sets the investigation-specific role assignments for users.
        /// </summary>
        DbSet<InvestigationRoleAssignment> InvestigationRoleAssignments { get; set; }

        /// <summary>
        /// Gets or sets the permission sets, which define collections of allowed and denied actions.
        /// </summary>
        DbSet<PermissionSet> PermissionSets { get; set; }

        /// <summary>
        /// Gets or sets the roles available in the system, each of which can have associated permissions.
        /// </summary>
        DbSet<Role> Roles { get; set; }

        /// <summary>
        /// Gets or sets the system actions that can be performed, used for permission and authorization checks.
        /// </summary>
        DbSet<SystemAction> SystemActions { get; set; }

        /// <summary>
        /// Gets or sets the system-wide role assignments for users.
        /// </summary>
        DbSet<SystemRoleAssignment> SystemRoleAssignments { get; set; }

        /// <summary>
        /// Gets or sets the refresh tokens issued to users for authentication.
        /// </summary>
        DbSet<UserRefreshToken> UserRefreshTokens { get; set; }

        /// <summary>
        /// Gets or sets the users registered in the system.
        /// </summary>
        DbSet<User> Users { get; set; }

        /// <summary>
        /// Gets or sets the user secrets, such as credentials or keys, associated with users.
        /// </summary>
        DbSet<UserSecret> UserSecrets { get; set; }
    }
}