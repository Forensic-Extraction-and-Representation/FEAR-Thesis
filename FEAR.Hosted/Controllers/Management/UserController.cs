using AutoMapper;
using FEAR.Api.Controllers;
using FEAR.Domain;
using FEAR.Domain.Helpers;
using FEAR.Host.Core.Api.Management.Security;
using FEAR.Host.Core.Database;
using FEAR.Host.Core.Identity;
using FEAR.Host.Core.ProtectedEncryptionKey;
using FEAR.Host.Domain.Api.Management.Security;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OpenIddict.Validation.AspNetCore;

namespace FEAR.Hosted.Controllers.Management
{
    [ApiController]
    [Route("v1.0/Management/[controller]/[action]")]
    public class UserController : AuthenticatedControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly IPasswordProvider _passwordProvider;
        private readonly IdentityDbContext _identityDbContext;
        private readonly IProtectedEncryptionKeyProvider _protectedEncryptionKeyProvider;
        private readonly IMapper _mapper;

        public UserController(
            IIdentityProvider identityProvider,
            IPermissionProvider permissionProvider,
            IdentityDbContext identityDbContext,
            IPasswordProvider passwordProvider,
            IConfiguration configuration,
            IProtectedEncryptionKeyProvider protectedEncryptionKeyProvider,
            IMapper mapper)
            : base(identityProvider, permissionProvider)
        {
            _mapper = mapper;
            _configuration = configuration;
            _passwordProvider = passwordProvider;
            _identityDbContext = identityDbContext;
            _protectedEncryptionKeyProvider = protectedEncryptionKeyProvider;
        }

        [HttpPost]
        [Authorize(AuthenticationSchemes = $"{OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme},{CookieAuthenticationDefaults.AuthenticationScheme}")]
        public IActionResult QueryUsers([FromBody] QueryUsersRequest request)
        {
            var listUserAttempt = CanPerformSystemAction(ActionDefinitions.System.User.List);
            if (!listUserAttempt.IsApproved)
                return Unauthorized();

            var usersQuery = _identityDbContext.Users.AsQueryable();
            if (!string.IsNullOrWhiteSpace(request?.UserNameFilter))
            {
                usersQuery = usersQuery.Where(u => u.UserName.Contains(request.UserNameFilter));
            }

            var users = usersQuery.ToList().Select(t => _mapper.Map<FEAR.Domain.Model.Authentication.User>(t));

            return Ok(new QueryUsersResponse
            {
                Results = users,
                TotalResults = users.Count(),
                ReturnedResults = users.Count()
            });
        }

        [HttpPost]
        [Authorize(AuthenticationSchemes = $"{OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme},{CookieAuthenticationDefaults.AuthenticationScheme}")]
        public IActionResult CreateUser([FromBody] CreateUserRequest request)
        {
            var createAttempt = CanPerformSystemAction(ActionDefinitions.System.User.Create);
            if (!createAttempt.IsApproved)
                return Unauthorized();

            // check if the username exists
            if (_identityDbContext.Users.Any(t => t.UserName == request.UserName))
                return BadRequest("Username already exists.");

            // check if the email exists
            if (_identityDbContext.Users.Any(t => t.Email == request.Email))
                return BadRequest("Email already exists.");

            var user = new FEAR.Domain.Dto.Authentication.User
            {
                UserName = request.UserName,
                Email = request.Email,
                CreatedDateTime = DateTime.UtcNow,
                SystemRoleAssignments = new List<FEAR.Domain.Dto.Authentication.SystemRoleAssignment>(),
                InvestigationRoleAssignments = new List<FEAR.Domain.Dto.Authentication.InvestigationRoleAssignment>(),
                UserSecret = null
            };

            using var txn = _identityDbContext.Database.BeginTransaction();
            _identityDbContext.Users.Add(user);
            _identityDbContext.SaveChanges();

            // Create UserSecret
            string passwordSalt = _passwordProvider.GenerateSalt();
            var passwordHash = _passwordProvider.HashPassword(request.Password, passwordSalt, 50000);

            var userSecretInfo = new FEAR.Domain.Model.Authentication.UserSecretInfo
            {
                PasswordHash = Convert.ToBase64String(passwordHash),
                Salt = passwordSalt,
                Iterations = 50000
            };

            var newUserSecret = new FEAR.Domain.Dto.Authentication.UserSecret
            {
                UserId = user.UserId,
                KeyName = _protectedEncryptionKeyProvider.DefaultKeyName
            };

            newUserSecret.SecretData = UserSecretHelpers.LockRecord(userSecretInfo, newUserSecret, _protectedEncryptionKeyProvider.EncryptData);
            _identityDbContext.UserSecrets.Add(newUserSecret);

            _identityDbContext.SaveChanges();
            txn.Commit();

            return Ok(new CreateUserResponse() { Success = true, UserId = user.UserId });
        }

        [HttpPost]
        [Authorize(AuthenticationSchemes = $"{OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme},{CookieAuthenticationDefaults.AuthenticationScheme}")]
        public IActionResult UpdateUser([FromBody] UpdateUserRequest request)
        {
            var updateAttempt = CanPerformSystemAction(ActionDefinitions.System.User.Update);
            if (!updateAttempt.IsApproved)
                return Unauthorized();

            var user = _identityDbContext.Users.FirstOrDefault(t => t.UserId == request.UserId);
            if (user == null)
                return NotFound();

            // Basic updates (excluding password)
            user.UserName = request.UserName;
            user.Email = request.Email;

            _identityDbContext.SaveChanges();
            return Ok(new UpdateUserResponse());
        }

        [HttpPost]
        [Authorize(AuthenticationSchemes = $"{OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme},{CookieAuthenticationDefaults.AuthenticationScheme}")]
        public IActionResult UpdatePassword([FromBody] UpdatePasswordRequest request)
        {
            var changePwAttempt = CanPerformSystemAction(ActionDefinitions.System.User.ChangePassword);
            if (!changePwAttempt.IsApproved)
                return Unauthorized();

            var user = _identityDbContext.Users.FirstOrDefault(t => t.UserId == request.UserId);
            if (user == null)
                return BadRequest("User does not exist.");

            var userSecret = _identityDbContext.UserSecrets.FirstOrDefault(t => t.UserId == request.UserId);
            if (userSecret == null)
            {
                userSecret = new FEAR.Domain.Dto.Authentication.UserSecret() { UserId = user.UserId };
                _identityDbContext.UserSecrets.Add(userSecret);
            }

            string passwordSalt = _passwordProvider.GenerateSalt();
            var passwordHash = _passwordProvider.HashPassword(request.NewPassword, passwordSalt, 50000);

            var userSecretInfo = new FEAR.Domain.Model.Authentication.UserSecretInfo
            {
                PasswordHash = Convert.ToBase64String(passwordHash),
                Salt = passwordSalt,
                Iterations = 50000
            };

            userSecret.SecretData = UserSecretHelpers.LockRecord(userSecretInfo, userSecret, _protectedEncryptionKeyProvider.EncryptData);
            _identityDbContext.SaveChanges();

            return Ok(new UpdatePasswordResponse());
        }

        [HttpPost]
        [Authorize(AuthenticationSchemes = $"{OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme},{CookieAuthenticationDefaults.AuthenticationScheme}")]
        public IActionResult QueryRoles([FromBody] FEAR.Host.Domain.Api.Management.Security.QueryRoles.Request request)
        {
            var canListRoles = CanPerformSystemAction(ActionDefinitions.System.Role.List);
            if (!canListRoles.IsApproved)
                return Unauthorized();

            var rolesQuery = _identityDbContext.Roles.AsQueryable();
            if (!string.IsNullOrWhiteSpace(request?.NameFilter))
            {
                rolesQuery = rolesQuery.Where(r => r.Name.Contains(request.NameFilter));
            }

            var roles = rolesQuery.ToList().Select(r => _mapper.Map<FEAR.Domain.Model.Authentication.Role>(r)).ToList();
            return Ok(new FEAR.Host.Domain.Api.Management.Security.QueryRoles.Response
            {
                Results = roles,
                ReturnedResults = roles.Count,
                TotalResults = roles.Count
            });
        }

        [HttpPost]
        [Authorize(AuthenticationSchemes = $"{OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme},{CookieAuthenticationDefaults.AuthenticationScheme}")]
        public IActionResult AssignSystemRoles([FromBody] AssignSystemRoles.Request request)
        {
            var manageAttempt = CanPerformSystemAction(ActionDefinitions.System.Role.Update);
            if (!manageAttempt.IsApproved)
                return Unauthorized();

            var user = _identityDbContext.Users.FirstOrDefault(t => t.UserId == request.UserId);
            if (user == null)
                return NotFound();

            var existing = _identityDbContext.SystemRoleAssignments.Where(s => s.UserId == request.UserId).ToList();
            if (existing.Any())
                _identityDbContext.SystemRoleAssignments.RemoveRange(existing);

            foreach (var roleId in request.RoleIds.Distinct())
            {
                _identityDbContext.SystemRoleAssignments.Add(new FEAR.Domain.Dto.Authentication.SystemRoleAssignment
                {
                    UserId = request.UserId,
                    RoleId = roleId
                });
            }
            _identityDbContext.SaveChanges();
            return Ok(new AssignSystemRoles.Response { Success = true });
        }

        [HttpPost]
        [Authorize(AuthenticationSchemes = $"{OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme},{CookieAuthenticationDefaults.AuthenticationScheme}")]
        public IActionResult RevokeSystemRoles([FromBody] RevokeSystemRoles.Request request)
        {
            var manageAttempt = CanPerformSystemAction(ActionDefinitions.System.Role.Update);
            if (!manageAttempt.IsApproved)
                return Unauthorized();
            var user = _identityDbContext.Users.FirstOrDefault(t => t.UserId == request.UserId);
            if (user == null)
                return NotFound();
            var existing = _identityDbContext.SystemRoleAssignments.Where(s => s.UserId == request.UserId).ToList();
            if (existing.Any())
                _identityDbContext.SystemRoleAssignments.RemoveRange(existing);
            _identityDbContext.SaveChanges();
            return Ok(new RevokeSystemRoles.Response { Success = true });
        }


        [HttpPost]
        [Authorize(AuthenticationSchemes = $"{OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme},{CookieAuthenticationDefaults.AuthenticationScheme}")]
        public IActionResult AssignInvestigationRoles([FromBody] AssignInvestigationRoles.Request request)
        {
            var manageAttempt = CanPerformSystemAction(ActionDefinitions.System.Investigation.Update);
            if (!manageAttempt.IsApproved)
                return Unauthorized();

            var user = _identityDbContext.Users.FirstOrDefault(t => t.UserId == request.UserId);
            if (user == null)
                return NotFound();

            var existing = _identityDbContext.InvestigationRoleAssignments
                .Where(s => s.UserId == request.UserId && s.InvestigationId == request.InvestigationId)
                .ToList();
            if (existing.Any())
                _identityDbContext.InvestigationRoleAssignments.RemoveRange(existing);
            
            _identityDbContext.InvestigationRoleAssignments.Add(new FEAR.Domain.Dto.Authentication.InvestigationRoleAssignment
            {
                UserId = request.UserId,
                RoleId = request.RoleId,
                InvestigationId = request.InvestigationId
            });

            _identityDbContext.SaveChanges();
            return Ok(new AssignInvestigationRoles.Response { Success = true });
        }

        [HttpPost]
        [Authorize(AuthenticationSchemes = $"{OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme},{CookieAuthenticationDefaults.AuthenticationScheme}")]
        public IActionResult RevokeInvestigationRoles([FromBody] RevokeInvestigationRoles.Request request)
        {
            var manageAttempt = CanPerformSystemAction(ActionDefinitions.System.Investigation.Update);
            if (!manageAttempt.IsApproved)
                return Unauthorized();
            var user = _identityDbContext.Users.FirstOrDefault(t => t.UserId == request.UserId);
            if (user == null)
                return NotFound();
            var existing = _identityDbContext.InvestigationRoleAssignments
                .Where(s => s.UserId == request.UserId && s.InvestigationId == request.InvestigationId)
                .ToList();
            if (existing.Any())
                _identityDbContext.InvestigationRoleAssignments.RemoveRange(existing);
            _identityDbContext.SaveChanges();
            return Ok(new RevokeInvestigationRoles.Response { Success = true });
        }

        [HttpPost]
        [Authorize(AuthenticationSchemes = $"{OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme},{CookieAuthenticationDefaults.AuthenticationScheme}")]
        public IActionResult QuerySystemUserRoles([FromBody] FEAR.Host.Domain.Api.Management.Security.QuerySystemUserRoles.Request request)
        {
            var perm = CanPerformSystemAction(ActionDefinitions.System.Role.List);
            if (!perm.IsApproved) return Unauthorized();

            var q = _identityDbContext.Users
                .Include(u => u.SystemRoleAssignments)
                .ThenInclude(a => a.Role)
                .AsQueryable();

            q = q.Where(u => u.UserId == request.UserId);

            var items = q.ToList().Select(u => new FEAR.Host.Domain.Api.Management.Security.QuerySystemUserRoles.SystemUserRoles
            {
                UserId = u.UserId,
                UserName = u.UserName,
                Email = u.Email,
                RoleIds = u.SystemRoleAssignments.Select(a => a.RoleId).ToList(),
                RoleNames = u.SystemRoleAssignments.Select(a => a.Role.Name).ToList()
            }).ToList();
            return Ok(new FEAR.Host.Domain.Api.Management.Security.QuerySystemUserRoles.Response { Results = items, ReturnedResults = items.Count, TotalResults = items.Count });
        }

        [HttpPost]
        [Authorize(AuthenticationSchemes = $"{OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme},{CookieAuthenticationDefaults.AuthenticationScheme}")]
        public IActionResult QueryInvestigationUserRoles([FromBody] QueryInvestigationUserRoles.Request request)
        {
            var perm = CanPerformInvestigationAction(ActionDefinitions.System.Investigation.Update, request.InvestigationId);
            if (!perm.IsApproved) return Unauthorized();

            var q = _identityDbContext.InvestigationRoleAssignments
                .Include(u => u.User)
                .Include(u=>u.Role)
                .AsQueryable();

            q = q.Where(t => t.InvestigationId == request.InvestigationId);

            var items = q.ToList().Select(u => new QueryInvestigationUserRoles.InvestigationUserRoles
            {
                UserId = u.UserId,
                InvestigationId = u.InvestigationId,
                RoleId = u.RoleId,
                RoleName = u.Role.Name,
                UserName = u.User.UserName
            }).ToList();

            return Ok(new QueryInvestigationUserRoles.Response { Results = items, ReturnedResults = items.Count, TotalResults = items.Count });
        }
    }
}
