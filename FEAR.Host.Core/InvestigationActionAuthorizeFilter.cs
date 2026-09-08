using FEAR.Domain.Database;
using FEAR.Host.Core;
using FEAR.Host.Core.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace FEAR.Host.Core
{
    /// <summary>
    /// Authorization filter for case or entity actions based on user permissions.
    /// Checks if the current user is allowed to perform a specified action on a given entity or case.
    /// </summary>
    public class InvestigationActionAuthorizeFilter : IAsyncAuthorizationFilter
    {
        private readonly IPermissionProvider _permissionProvider;
        private readonly IIdentityProvider _identityProvider;
        private readonly IInvestigationManagementDbContext _mgDbContext;
        private readonly string _action;
        private readonly string _entityIdRouteParam;

        /// <summary>
        /// Initializes a new instance of the <see cref="InvestigationActionAuthorizeFilter"/> class.
        /// </summary>
        /// <param name="action">The action name to authorize (e.g., "View", "Edit").</param>
        /// <param name="entityIdRouteParam">The route parameter name that contains the entity or case ID.</param>
        /// <param name="permissionProvider">The permission provider for action checks.</param>
        /// <param name="identityProvider">The identity provider for user information.</param>
        /// <param name="mgDbContext">The investigation management database context.</param>
        public InvestigationActionAuthorizeFilter(
            string action,
            string entityIdRouteParam,
            IPermissionProvider permissionProvider,
            IIdentityProvider identityProvider,
            IInvestigationManagementDbContext mgDbContext)
        {
            _permissionProvider = permissionProvider;
            _action = action;
            _entityIdRouteParam = entityIdRouteParam;
            _identityProvider = identityProvider;
            _mgDbContext = mgDbContext;
        }

        /// <summary>
        /// Called to authorize the current request based on user permissions and entity context.
        /// </summary>
        /// <param name="filterContext">The authorization filter context.</param>
        public async Task OnAuthorizationAsync(AuthorizationFilterContext filterContext)
        {
            // Try to get entity ID from route data
            if (!filterContext.RouteData.Values.TryGetValue(_entityIdRouteParam, out var entityIdObj))
            {
                filterContext.Result = new BadRequestResult(); // or Forbid
                return;
            }

            Guid.TryParse(entityIdObj?.ToString(), out var entityId);

            var claimsUser = filterContext.HttpContext.User;
            var currentUserInfos = _identityProvider.GetUserInfoFromClaim(claimsUser);
            var currentUser = _identityProvider.GetUserFromClaim(currentUserInfos);

            // If entityId is not a valid Guid, try to resolve by name
            if (entityId == Guid.Empty)
                entityId = _mgDbContext.GetInvestigationByName(entityIdObj?.ToString())?.InvestigationId ?? Guid.Empty;
            else
            {
                entityId = _mgDbContext.GetInvestigationByName(entityIdObj?.ToString())?.InvestigationId ?? entityId;
            }

            PerformActionResult result = _permissionProvider.CanPerformInvestigationAction(currentUser, _action, entityId);
            bool allowed = result.IsApproved;

            if (!allowed)
            {
                filterContext.Result = new ForbidResult();
            }
        }
    }
}