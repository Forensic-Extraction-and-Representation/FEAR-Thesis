using FEAR.Host.Domain.Api.Authentication;
using FEAR.Domain.Dto.Authentication;
using FEAR.Host.Core;
using FEAR.Host.Core.Identity;
using Microsoft.AspNetCore.Mvc;

namespace FEAR.Api.Controllers
{
    /// <summary>
    /// Base controller for API endpoints that require an authenticated user.
    /// Provides convenient access to the current user's information and permission checks
    /// using the supplied identity and permission providers.
    /// </summary>
    public abstract class AuthenticatedControllerBase : ControllerBase
    {
        /// <summary>
        /// Lazily retrieves the <see cref="UserInfo"/> for the current authenticated user
        /// from the claims principal.
        /// </summary>
        protected readonly Lazy<UserInfo[]> CurrentUserInfo = null;

        /// <summary>
        /// Lazily retrieves the <see cref="User"/> domain entity for the current authenticated user
        /// using the resolved <see cref="UserInfo"/>.
        /// </summary>
        protected readonly Lazy<User> CurrentUser = null;

        /// <summary>
        /// The identity provider used to resolve user information and domain user entities.
        /// </summary>
        protected readonly IIdentityProvider _identityProvider;

        /// <summary>
        /// The permission provider used to check user permissions for system and investigation actions.
        /// </summary>
        protected readonly IPermissionProvider _permissionProvider;
        
        /// <summary>
        /// Initializes a new instance of the <see cref="AuthenticatedControllerBase"/> class.
        /// </summary>
        /// <param name="identityProvider">The identity provider for user resolution.</param>
        /// <param name="permissionProvider">The permission provider for permission checks.</param>
        public AuthenticatedControllerBase(IIdentityProvider identityProvider, IPermissionProvider permissionProvider)
        {
            _identityProvider = identityProvider;
            _permissionProvider = permissionProvider;
            CurrentUserInfo = new Lazy<UserInfo[]>(() => _identityProvider.GetUserInfoFromClaim(User));
            CurrentUser = new Lazy<User>(() => _identityProvider.GetUserFromClaim(CurrentUserInfo.Value));
        }

        /// <summary>
        /// Checks if the current user can perform the specified system action.
        /// </summary>
        /// <param name="action">The system action to check.</param>
        /// <returns>A <see cref="PerformActionResult"/> indicating if the action is permitted.</returns>
        protected PerformActionResult CanPerformSystemAction(SystemAction action)
        {
            return _permissionProvider.CanPerformSystemAction(CurrentUser.Value, action.Name);
        }

        /// <summary>
        /// Checks if the current user can perform the specified investigation action within a given investigation.
        /// </summary>
        /// <param name="action">The investigation action to check.</param>
        /// <param name="investigationId">The unique identifier of the investigation context.</param>
        /// <returns>A <see cref="PerformActionResult"/> indicating if the action is permitted.</returns>
        protected PerformActionResult CanPerformInvestigationAction(SystemAction action, Guid investigationId)
        {
            return _permissionProvider.CanPerformInvestigationAction(CurrentUser.Value, action.Name, investigationId);
        }
    }
}