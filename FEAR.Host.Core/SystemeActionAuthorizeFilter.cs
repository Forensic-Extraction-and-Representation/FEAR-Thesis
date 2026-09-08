using FEAR.Host.Core.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace FEAR.Host.Core
{
    /// <summary>
    /// Authorization filter for system-level actions based on user permissions.
    /// Checks if the current user is allowed to perform a specified system action.
    /// </summary>
    public class SystemeActionAuthorizeFilter : IAsyncAuthorizationFilter
    {
        private readonly IPermissionProvider _permissionProvider;
        private readonly IIdentityProvider _identityProvider;
        private readonly string _action;

        /// <summary>
        /// Initializes a new instance of the <see cref="SystemeActionAuthorizeFilter"/> class.
        /// </summary>
        /// <param name="action">The system action name to authorize (e.g., "System.View", "System.Edit").</param>
        /// <param name="permissionProvider">The permission provider for action checks.</param>
        /// <param name="identityProvider">The identity provider for user information.</param>
        public SystemeActionAuthorizeFilter(string action, IPermissionProvider permissionProvider, IIdentityProvider identityProvider)
        {
            _permissionProvider = permissionProvider;
            _action = action;
            _identityProvider = identityProvider;
        }

        /// <summary>
        /// Called to authorize the current request based on user permissions and the specified system action.
        /// </summary>
        /// <param name="filterContext">The authorization filter context.</param>
        public async Task OnAuthorizationAsync(AuthorizationFilterContext filterContext)
        {
            var claimsUser = filterContext.HttpContext.User;
            var currentUserInfos = _identityProvider.GetUserInfoFromClaim(claimsUser);
            var currentUser = _identityProvider.GetUserFromClaim(currentUserInfos);
            var result = _permissionProvider.CanPerformSystemAction(currentUser, _action);

            bool allowed = result.IsApproved;

            if (!allowed)
            {
                filterContext.Result = new ForbidResult();
            }
        }
    }
}