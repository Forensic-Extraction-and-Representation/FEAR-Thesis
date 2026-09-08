using FEAR.Host.Domain.Api.Authentication;
using FEAR.Host.Core.Database;
using FEAR.Host.Core.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FEAR.Api.Controllers
{
    /// <summary>
    /// API controller for authentication-related operations such as login, token refresh, and retrieving current user info.
    /// Handles user authentication, token issuance, and user info extraction from claims.
    /// </summary>
    [ApiController]
    [Route("v1.0/[controller]/[action]")]
    public class AuthenticationController : ControllerBase
    {
        /// <summary>
        /// Logger for diagnostic and audit purposes.
        /// </summary>
        private readonly ILogger<AuthenticationController> _logger;

        /// <summary>
        /// Application configuration, used for retrieving authentication settings.
        /// </summary>
        private readonly IConfiguration _config;

        /// <summary>
        /// Provides permission checks for system and investigation actions.
        /// </summary>
        private readonly IPermissionProvider _permissionProvider;

        /// <summary>
        /// Database context for identity and authentication data.
        /// </summary>
        private readonly IdentityDbContext _identityDbContext;

        /// <summary>
        /// Provides identity and authentication operations such as login and token refresh.
        /// </summary>
        private readonly IIdentityProvider _identityProvider;

        /// <summary>
        /// Gets the default encryption key name from configuration.
        /// </summary>
        private string DefaultKeyName => _config.GetValue<string>("Authentication:EncryptionKeyName");

        /// <summary>
        /// Initializes a new instance of the <see cref="AuthenticationController"/> class.
        /// </summary>
        /// <param name="logger">Logger for diagnostics.</param>
        /// <param name="config">Application configuration.</param>
        /// <param name="identityProvider">Identity provider for authentication operations.</param>
        /// <param name="permissionProvider">Permission provider for authorization checks.</param>
        /// <param name="identityDbContext">Database context for identity data.</param>
        public AuthenticationController(
            ILogger<AuthenticationController> logger,
            IConfiguration config,
            IIdentityProvider identityProvider,
            IPermissionProvider permissionProvider,
            IdentityDbContext identityDbContext)
        {
            _logger = logger;
            _config = config;
            _permissionProvider = permissionProvider;
            _identityDbContext = identityDbContext;
            _identityProvider = identityProvider;
        }

        /// <summary>
        /// Gets the current authenticated user's information from the claims principal.
        /// </summary>
        /// <returns>User information for the current user.</returns>
        [HttpGet(Name = "CurrentUserInfo")]
        public UserInfo[] CurrentUserInfo()
        {
            return _identityProvider.GetUserInfoFromClaim(User);
        }

        /// <summary>
        /// Authenticates a user and issues a JWT and refresh token if successful.
        /// Returns 401 Unauthorized if authentication fails.
        /// </summary>
        /// <param name="loginRequest">The login request containing username and password.</param>
        /// <returns>A login response with tokens if successful, or Unauthorized if not.</returns>
        [AllowAnonymous]
        [HttpPost(Name = "CreateToken")]
        public async Task<IActionResult> CreateToken(LoginRequest loginRequest)
        {
            DateTime startTicks = DateTime.Now;

            var response = await _identityProvider.Login(loginRequest);

            if (response.Token != null)
            {
                return Ok(response);
            }

            // Ensure requests are all processed in a uniform manner to mitigate timing attacks
            int waitTicks = 1000 - (DateTime.Now - startTicks).Milliseconds;
            if (waitTicks > 0)
                Thread.Sleep(waitTicks);

            return Unauthorized();
        }

        /// <summary>
        /// Refreshes a JWT using a valid refresh token, issuing a new JWT and refresh token.
        /// Returns 401 Unauthorized if the refresh token is invalid or expired.
        /// </summary>
        /// <param name="refreshRequest">The refresh request containing the refresh token.</param>
        /// <returns>A refresh response with new tokens if successful, or Unauthorized if not.</returns>
        [AllowAnonymous]
        [HttpPost(Name = "RefreshRequest")]
        public async Task<IActionResult> RefreshToken(RefreshRequest refreshRequest)
        {
            DateTime startTicks = DateTime.Now;

            var response = await _identityProvider.Refresh(refreshRequest);

            if (response.Token != null)
            {
                return Ok(response);
            }

            // Ensure requests are all processed in a uniform manner to mitigate timing attacks
            int waitTicks = 1000 - (DateTime.Now - startTicks).Milliseconds;
            if (waitTicks > 0)
                Thread.Sleep(waitTicks);

            return Unauthorized();
        }
    }
}
