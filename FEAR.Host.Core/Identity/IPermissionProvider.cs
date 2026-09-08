using FEAR.Domain.Dto.Authentication;

namespace FEAR.Host.Core.Identity
{
    /// <summary>
    /// Defines a contract for checking user permissions for system and investigation actions.
    /// Provides methods to determine if a user can perform a specific action, returning details about approving and denying roles.
    /// </summary>
    public interface IPermissionProvider
    {
        /// <summary>
        /// Determines whether the specified user can perform a given system-wide action.
        /// </summary>
        /// <param name="user">The user whose permissions are being checked.</param>
        /// <param name="actionName">The name of the system action to check.</param>
        /// <returns>
        /// A <see cref="PerformActionResult"/> indicating whether the action is approved,
        /// and listing the roles that approve or deny the action.
        /// </returns>
        PerformActionResult CanPerformSystemAction(User user, string actionName);

        /// <summary>
        /// Determines whether the specified user can perform a given action within a specific investigation context.
        /// </summary>
        /// <param name="user">The user whose permissions are being checked.</param>
        /// <param name="actionName">The name of the investigation action to check.</param>
        /// <param name="investigationId">The unique identifier of the investigation context.</param>
        /// <returns>
        /// A <see cref="PerformActionResult"/> indicating whether the action is approved,
        /// and listing the roles that approve or deny the action within the investigation.
        /// </returns>
        PerformActionResult CanPerformInvestigationAction(User user, string actionName, Guid investigationId);

        /// <summary>
        /// Determines whether the specified user can perform any of the given system-wide actions.
        /// </summary>
        /// <param name="user">The user whose permissions are being checked.</param>
        /// <param name="actionNames">The names of the system actions to check.</param>
        /// <returns>
        /// A <see cref="PerformActionResult"/> indicating whether any of the actions are approved
        /// </returns>
        PerformActionResult CanPerformAnySystemAction(User user, IEnumerable<string> actionNames);

        /// <summary>
        /// Determines whether the specified user can perform any of the given actions within a specific investigation context.
        /// </summary>
        /// <param name="user">The user whose permissions are being checked.</param>
        /// <param name="actionNames">The names of the investigation actions to check.</param>
        /// <param name="investigationId">The unique identifier of the investigation context.</param>
        /// <returns>
        /// A <see cref="PerformActionResult"/> indicating whether any of the actions are approved
        /// </returns>
        PerformActionResult CanPerformAnyInvestigationAction(User user, IEnumerable<string> actionNames, Guid investigationId);
    }
}
