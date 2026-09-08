using FEAR.Api.Controllers;
using FEAR.Host.Core.Identity;
using Microsoft.AspNetCore.Mvc;

namespace FEAR.Hosted.Controllers.Management
{
    [ApiController]
    [Route("v1.0/Management/[controller]/[action]")]
    public class QueryController : AuthenticatedControllerBase
    {
        public QueryController(IIdentityProvider identityProvider, IPermissionProvider permissionProvider)
            : base(identityProvider, permissionProvider)
        {
        }
    }
}
