using FEAR.Domain.Dto.Authentication;
using FEAR.Host.Core.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace FEAR.Host.Core.Identity
{
    /// <summary>
    /// Provides permission checks for system and investigation actions.
    /// Determines which roles approve or deny a user's ability to perform specific actions.
    /// </summary>
    public class PermissionProvider : IPermissionProvider
    {
        private readonly IdentityDbContext _identityDbContext;
        private readonly InvestigationManagementDbContext _investigationManagementDbContext;
        private readonly IConfiguration _configuration;

        /// <summary>
        /// Initializes a new instance of the <see cref="PermissionProvider"/> class.
        /// </summary>
        /// <param name="identityDbContext">The identity database context.</param>
        /// <param name="configuration">The application configuration.</param>
        public PermissionProvider(IdentityDbContext identityDbContext, InvestigationManagementDbContext investigationManagementDbContext,  IConfiguration configuration)
        {
            _identityDbContext = identityDbContext;
            _investigationManagementDbContext = investigationManagementDbContext;
            _configuration = configuration;
        }

        /// <summary>
        /// Determines whether the specified user can perform a given action within a specific investigation context.
        /// Returns the roles that approve or deny the action.
        /// </summary>
        /// <param name="user">The user whose permissions are being checked.</param>
        /// <param name="actionName">The name of the investigation action to check.</param>
        /// <param name="investigationId">The unique identifier of the investigation context.</param>
        /// <returns>
        /// A <see cref="PerformActionResult"/> indicating whether the action is approved,
        /// and listing the roles that approve or deny the action within the investigation.
        /// </returns>
        public PerformActionResult CanPerformInvestigationAction(User user, string actionName, Guid investigationId)
        {
            if (user == null)
            {
                return new PerformActionResult(new List<string>(), new List<string> { "Not Authenticated" });
            }

            if(_investigationManagementDbContext.Investigations.Find(investigationId) == null)
            {
                return new PerformActionResult(new List<string>(), new List<string> { "Investigation Not Found" });
            }

            var dbUser = _identityDbContext.Users.FirstOrDefault(t => t.UserId == user.UserId);

            var roleAssignments = _identityDbContext.InvestigationRoleAssignments.Where(t => t.InvestigationId == investigationId)
                .Include(t => t.Role).ThenInclude(t => t.Permissions).ThenInclude(t => t.ApprovedActions)
                .Include(t => t.Role).ThenInclude(t => t.Permissions).ThenInclude(t => t.DeniedActions)
                .AsNoTracking();

            var rolesApproving = roleAssignments
                .Where(t => t.UserId == dbUser.UserId && t.Role.Permissions.Any(p => p.ApprovedActions.Any(a => a.Name == actionName)))
                .ToList();
            var rolesDenying = roleAssignments
                .Where(t => t.UserId == dbUser.UserId && t.Role.Permissions.Any(p => p.DeniedActions.Any(a => a.Name == actionName)))
                .ToList();

            // Add the admin role to the list of approving roles if the user is a system admin
            var adminRole = _identityDbContext.Roles.FirstOrDefault(t => t.Name == "SystemAdmin");
            if (adminRole != null)
            {
                var adminRoleAssignments = _identityDbContext.SystemRoleAssignments
                    .Where(t => t.UserId == dbUser.UserId && t.RoleId == adminRole.RoleId)
                    .AsNoTracking();
                if (adminRoleAssignments.Any())
                {
                    rolesApproving.Add(new InvestigationRoleAssignment
                    {
                        Role = adminRole,
                        UserId = dbUser.UserId,
                        InvestigationId = investigationId
                    });
                }
            }

            return new PerformActionResult(rolesApproving.Select(t => t.Role.Name), rolesDenying.Select(t => t.Role.Name));
        }

        /// <summary>
        /// Determines whether the specified user can perform any of the given actions within a specific investigation context.
        /// </summary>
        /// <param name="user">The user whose permissions are being checked.</param>
        /// <param name="actionNames">The names of the investigation actions to check.</param>
        /// <param name="investigationId">The unique identifier of the investigation context.</param>
        /// <returns>
        /// A <see cref="PerformActionResult"/> indicating whether any of the actions are approved
        /// </returns>
        public PerformActionResult CanPerformAnyInvestigationAction(User user, IEnumerable<string> actionNames, Guid investigationId)
        {
            if (user == null)
            {
                return new PerformActionResult(new List<string>(), new List<string> { "Not Authenticated" });
            }

            if (_investigationManagementDbContext.Investigations.Find(investigationId) == null)
            {
                return new PerformActionResult(new List<string>(), new List<string> { "Investigation Not Found" });
            }

            var dbUser = _identityDbContext.Users.FirstOrDefault(t => t.UserId == user.UserId);
            var roleAssignments = _identityDbContext.InvestigationRoleAssignments.Where(t => t.InvestigationId == investigationId)
                .Include(t => t.Role).ThenInclude(t => t.Permissions).ThenInclude(t => t.ApprovedActions)
                .Include(t => t.Role).ThenInclude(t => t.Permissions).ThenInclude(t => t.DeniedActions)
                .AsNoTracking();
            var rolesApproving = roleAssignments
                .Where(t => t.UserId == dbUser.UserId && t.Role.Permissions.Any(p => p.ApprovedActions.Any(a => actionNames.Contains(a.Name))))
                .ToList();
            var rolesDenying = roleAssignments
                .Where(t => t.UserId == dbUser.UserId && t.Role.Permissions.Any(p => p.DeniedActions.Any(a => actionNames.Contains(a.Name))))
                .ToList();
            // Add the admin role to the list of approving roles if the user is a system admin
            var adminRole = _identityDbContext.Roles.FirstOrDefault(t => t.Name == "SystemAdmin");
            if (adminRole != null)
            {
                var adminRoleAssignments = _identityDbContext.SystemRoleAssignments
                    .Where(t => t.UserId == dbUser.UserId && t.RoleId == adminRole.RoleId)
                    .AsNoTracking();
                if (adminRoleAssignments.Any())
                {
                    rolesApproving.Add(new InvestigationRoleAssignment
                    {
                        Role = adminRole,
                        UserId = dbUser.UserId,
                        InvestigationId = investigationId
                    });
                }
            }
            return new PerformActionResult(rolesApproving.Select(t => t.Role.Name), rolesDenying.Select(t => t.Role.Name));
        }

        /// <summary>
        /// Determines whether the specified user can perform a given system-wide action.
        /// Returns the roles that approve or deny the action.
        /// </summary>
        /// <param name="user">The user whose permissions are being checked.</param>
        /// <param name="actionName">The name of the system action to check.</param>
        /// <returns>
        /// A <see cref="PerformActionResult"/> indicating whether the action is approved,
        /// and listing the roles that approve or deny the action.
        /// </returns>
        public PerformActionResult CanPerformSystemAction(User user, string actionName)
        {
            if (user == null)
            {
                return new PerformActionResult(new List<string>(), new List<string> { "Not Authenticated" });
            }

            var dbUser = _identityDbContext.Users.FirstOrDefault(t => t.UserId == user.UserId);

            var roleAssignments = _identityDbContext.SystemRoleAssignments
                .Include(t => t.Role).ThenInclude(t => t.Permissions).ThenInclude(t => t.ApprovedActions)
                .Include(t => t.Role).ThenInclude(t => t.Permissions).ThenInclude(t => t.DeniedActions)
                .AsNoTracking();

            var rolesApproving = roleAssignments
                .Where(t => t.UserId == dbUser.UserId && t.Role.Permissions.Any(p => p.ApprovedActions.Any(a => a.Name == actionName)))
                .ToList();
            var rolesDenying = roleAssignments
                .Where(t => t.UserId == dbUser.UserId && t.Role.Permissions.Any(p => p.DeniedActions.Any(a => a.Name == actionName)))
                .ToList();

            return new PerformActionResult(rolesApproving.Select(t => t.Role.Name), rolesDenying.Select(t => t.Role.Name));
        }

        /// <summary>
        /// Determines whether the specified user can perform any of the given system-wide actions.
        /// </summary>
        /// <param name="user">The user whose permissions are being checked.</param>
        /// <param name="actionNames">The names of the system actions to check.</param>
        /// <returns>
        /// A <see cref="PerformActionResult"/> indicating whether any of the actions are approved
        /// </returns>
        public PerformActionResult CanPerformAnySystemAction(User user, IEnumerable<string> actionNames)
        {
            if (user == null)
            {
                return new PerformActionResult(new List<string>(), new List<string> { "Not Authenticated" });
            }
            var dbUser = _identityDbContext.Users.FirstOrDefault(t => t.UserId == user.UserId);
            var roleAssignments = _identityDbContext.SystemRoleAssignments
                .Include(t => t.Role).ThenInclude(t => t.Permissions).ThenInclude(t => t.ApprovedActions)
                .Include(t => t.Role).ThenInclude(t => t.Permissions).ThenInclude(t => t.DeniedActions)
                .AsNoTracking();
            var rolesApproving = roleAssignments
                .Where(t => t.UserId == dbUser.UserId && t.Role.Permissions.Any(p => p.ApprovedActions.Any(a => actionNames.Contains(a.Name))))
                .ToList();
            var rolesDenying = roleAssignments
                .Where(t => t.UserId == dbUser.UserId && t.Role.Permissions.Any(p => p.DeniedActions.Any(a => actionNames.Contains(a.Name))))
                .ToList();
            return new PerformActionResult(rolesApproving.Select(t => t.Role.Name), rolesDenying.Select(t => t.Role.Name));
        }
    }
}
