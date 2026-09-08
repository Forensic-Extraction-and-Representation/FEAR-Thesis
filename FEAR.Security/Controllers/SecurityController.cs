using FEAR.Domain.Model.Authentication;
using FEAR.Host.Core;
using FEAR.Host.Core.Identity;
using Microsoft.AspNetCore.Mvc;

namespace FEAR.Security.Controllers
{
    public abstract class SecurityController : Controller
    {
        protected readonly IIdentityProvider _identityProvider;
        protected readonly IPermissionProvider _permissionProvider;

        public SecurityController(IIdentityProvider identityProvider, IPermissionProvider permissionProvider)
        {
            _permissionProvider = permissionProvider;
            _identityProvider = identityProvider;
        }

        protected PerformActionResult CanPerformSystemAction(SystemAction action)
        {
            var userInfo = _identityProvider.GetUserInfoFromClaim(User);
            FEAR.Domain.Dto.Authentication.User user = _identityProvider.GetUserFromClaim(userInfo);
            return _permissionProvider.CanPerformSystemAction(user, action.Name);
        }
    }
}
