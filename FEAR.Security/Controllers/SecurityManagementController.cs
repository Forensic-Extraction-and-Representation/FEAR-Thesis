using AutoMapper;
using FEAR.Domain;
using FEAR.Host.Core.Api.Management.Security;
using FEAR.Host.Core.Database;
using FEAR.Host.Core.Identity;
using FEAR.Host.Core.ProtectedEncryptionKey;
using Microsoft.AspNetCore.Mvc;
using FEAR.Host.Domain.Api.Management.Security;
using FEAR.Domain.Helpers;
using FEAR.Domain.Model.Authentication;

namespace FEAR.Security.Controllers
{
    [ApiController]
    [Route("v1.0/[controller]/[action]")]
    public class SecurityManagementController : SecurityController
    {
        private readonly IConfiguration _configuration;
        private readonly IPasswordProvider _passwordProvider;
        private readonly IdentityDbContext _identityDbContext;
        private IProtectedEncryptionKeyProvider _protectedEncryptionKeyProvider;
        private readonly IMapper _mapper;

        public SecurityManagementController(IIdentityProvider identifyProvider, IdentityDbContext identityDbContext, IPasswordProvider passwordProvider,
            IConfiguration configuration, IPermissionProvider permissionProvider, IProtectedEncryptionKeyProvider protectedEncryptionKeyProvider, IMapper mapper) : base(identifyProvider, permissionProvider)
        {
            _mapper = mapper;
            _configuration = configuration;
            _passwordProvider = passwordProvider;
            _identityDbContext = identityDbContext;
            _protectedEncryptionKeyProvider = protectedEncryptionKeyProvider;

            if (!_configuration.GetSection("SecurityManagement").Get<SecurityManagementBuilderOptions>().Enabled)
                throw new NotImplementedException();
        }

        [HttpPost()]
        public IActionResult QueryUsers([FromForm] QueryUsersRequest request)
        {
            var listUserAttempt = CanPerformSystemAction(ActionDefinitions.System.User.List);
            if (listUserAttempt.IsApproved)
            {
                IEnumerable<FEAR.Domain.Model.Authentication.User> users = _identityDbContext.Users.Select(t => _mapper.Map<FEAR.Domain.Model.Authentication.User>(t));

                return Ok(new QueryUsersResponse
                {
                    Results = users,
                    TotalResults = users.Count(),
                    ReturnedResults = users.Count()
                });
            }

            return Unauthorized();
        }

        [HttpPost()]
        public IActionResult CreateUser([FromForm] CreateUserRequest request)
        {
            var listUserAttempt = CanPerformSystemAction(ActionDefinitions.System.User.Create);
            if (listUserAttempt.IsApproved)
            {
                // check if the username exists
                if (_identityDbContext.Users.Any(t => t.UserName == request.UserName))
                    return BadRequest("Username already exists.");

                // check if the email exists
                if (_identityDbContext.Users.Any(t => t.Email == request.Email))
                    return BadRequest("Email already exists.");

                // create the user
                FEAR.Domain.Dto.Authentication.User user = _mapper.Map<FEAR.Domain.Dto.Authentication.User>(request);

                // Begin transaction
                var txn = _identityDbContext.Database.BeginTransaction();
                _identityDbContext.Users.Add(user);

                // Save changes
                _identityDbContext.SaveChanges();

                // Create UserSecret
                string passwordSalt = _passwordProvider.GenerateSalt();
                var passwordHash = _passwordProvider.HashPassword(request.Password, passwordSalt, 50000);

                var userSecretInfo = new UserSecretInfo
                {
                    PasswordHash = Convert.ToBase64String(passwordHash),
                    Salt = passwordSalt,
                    Iterations = 50000
                };

                var newUserSecret = new FEAR.Domain.Dto.Authentication.UserSecret
                {
                    UserId = user.UserId,
                };

                newUserSecret.SecretData = UserSecretHelpers.LockRecord(userSecretInfo, newUserSecret, _protectedEncryptionKeyProvider.EncryptData);
                _identityDbContext.UserSecrets.Add(newUserSecret);

                // Save changes
                _identityDbContext.SaveChanges();

                // Commit transaction
                txn.Commit();

                return Ok(new CreateUserResponse() { Success = true, UserId = user.UserId });
            }
            return Unauthorized();
        }

        [HttpPost()]
        public IActionResult UpdateUser([FromForm] UpdateUserRequest request)
        {
            return Unauthorized();
        }

        [HttpPost()]
        public IActionResult UpdatePassword([FromForm] UpdatePasswordRequest request)
        {
            var listUserAttempt = CanPerformSystemAction(ActionDefinitions.System.User.ChangePassword);
            if (listUserAttempt.IsApproved)
            {
                var user = _identityDbContext.Users.FirstOrDefault(t => t.UserId == request.UserId);
                if (user == null)
                    return BadRequest("User does not exist.");

                var userSecret = _identityDbContext.UserSecrets.FirstOrDefault(t => t.UserId == request.UserId);
                if (userSecret == null)
                {
                    userSecret = new FEAR.Domain.Dto.Authentication.UserSecret()
                    {
                        UserId = user.UserId
                    };
                    _identityDbContext.UserSecrets.Add(userSecret);
                }

                // Create UserSecret
                string passwordSalt = _passwordProvider.GenerateSalt();
                var passwordHash = _passwordProvider.HashPassword(request.NewPassword, passwordSalt, 50000);

                var userSecretInfo = new UserSecretInfo
                {
                    PasswordHash = Convert.ToBase64String(passwordHash),
                    Salt = passwordSalt,
                    Iterations = 50000
                }
                ;
                userSecret.SecretData = UserSecretHelpers.LockRecord(userSecretInfo, userSecret, _protectedEncryptionKeyProvider.EncryptData);

                // Save changes
                _identityDbContext.SaveChanges();
            }

            return Unauthorized();
        }
    }
}
