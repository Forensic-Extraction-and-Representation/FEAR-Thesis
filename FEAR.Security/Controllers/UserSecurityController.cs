using FEAR.Host.Core.Identity;
using Microsoft.AspNetCore.Mvc;

namespace FEAR.Security.Controllers
{
    [ApiController]
    [Route("v1.0/[controller]/[action]")]
    public class UserSecurityController : Controller
    {
        private readonly IIdentityProvider _identityProvider;
        private readonly IConfiguration _configuration;

        public UserSecurityController(IIdentityProvider identifyProvider, IConfiguration configuration) {
            _identityProvider = identifyProvider;
            _configuration = configuration;
        }

        [HttpGet()]
        public IActionResult ChangePassword()
        {
            return View();
        }
    }
}
