using FEAR.Domain.Dto.Authentication;
using FEAR.Domain.Helpers;
using FEAR.Host.Core.Database;
using FEAR.Host.Core.ProtectedEncryptionKey;
using FEAR.Host.Domain.Api.Authentication;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace FEAR.Host.Core.Identity
{

    /// <summary>
    /// Implements <see cref="IIdentityProvider"/> for authentication and user management.
    /// </summary>
    public class IdentityProvider : IIdentityProvider
    {
        private readonly IProtectedEncryptionKeyProvider _protectedEncryptionKeyProvider;
        private readonly IdentityDbContext _identityDbContext;
        private readonly IPasswordProvider _passwordProvider;
        private readonly IConfiguration _config;
        private TimeSpan ExpirationTime => _expirationTime.Value;

        private readonly Lazy<TimeSpan> _expirationTime;

        /// <summary>
        /// Initializes a new instance of the <see cref="IdentityProvider"/> class.
        /// </summary>
        public IdentityProvider(IdentityDbContext identityDbContext, IProtectedEncryptionKeyProvider protectedEncryptionKeyProvider,
            IConfiguration configuration, IPasswordProvider passwordProvider)
        {
            _identityDbContext = identityDbContext;
            _passwordProvider = passwordProvider;
            _protectedEncryptionKeyProvider = protectedEncryptionKeyProvider;
            _config = configuration;

            _expirationTime = new Lazy<TimeSpan>(() =>
            {
                TimeSpan expTime = TimeSpan.FromMinutes(60);
                var configValue = _config["Jwt:ExpirationTimeInMinutes"];
                if (!string.IsNullOrEmpty(configValue))
                {
                    if (!int.TryParse(configValue, out int intValue))
                        expTime = TimeSpan.FromMinutes(intValue);
                }

                return expTime;
            });
        }

        /// <summary>
        /// Authenticates a user and issues a JWT and refresh token if successful.
        /// </summary>
        /// <param name="loginRequest">The login request containing username and password.</param>
        /// <returns>A <see cref="LoginResponse"/> with token and user info if successful.</returns>
        public async Task<LoginResponse> Login(LoginRequest loginRequest)
        {
            var rtCleanup = CleanupRefreshTokensAsync();

            // Get the user from the repository based on the username
            User? user = _identityDbContext.Users.FirstOrDefault(x => x.UserName == loginRequest.Username);
            if (user != null)
            {
                // Get the user secret from the repository based on the username
                var secret = _identityDbContext.UserSecrets.FirstOrDefault(x => x.UserId == user.UserId);

                // unlock the record
                var userSecretInfo = UserSecretHelpers.UnlockRecord(secret, _protectedEncryptionKeyProvider.DecryptData);

                // check the password matches
                if (loginRequest.Username.Equals(user.UserName, StringComparison.OrdinalIgnoreCase) && _passwordProvider.IsPasswordMatch(loginRequest.Password, userSecretInfo))
                {
                    var tokenDescriptor = CreateClaimTokenForUser(user);
                    var refreshToken = Guid.NewGuid().ToString();

                    var tokenHandler = new JwtSecurityTokenHandler();
                    var token = tokenHandler.CreateToken(tokenDescriptor);
                    var stringToken = tokenHandler.WriteToken(token);

                    await rtCleanup;

                    _identityDbContext.UserRefreshTokens.Add(new UserRefreshToken() { UserId = user.UserId, RefreshToken = EncodeRefreshToken(refreshToken), ValidTo = DateTime.UtcNow.AddDays(1) });
                    _identityDbContext.SaveChanges();

                    return new LoginResponse() { IsSuccess = true, Token = stringToken, UserIdentity = tokenDescriptor.Subject, Expires = tokenDescriptor.Expires, RefreshToken = refreshToken, UserId = user.UserId };
                }
            }

            await rtCleanup;
            return new LoginResponse() { IsSuccess = false, Token = null, Expires = null };
        }

        /// <summary>
        /// Refreshes a JWT using a valid refresh token, issuing a new JWT and refresh token.
        /// </summary>
        /// <param name="refreshTokenRequest">The refresh request containing the refresh token.</param>
        /// <returns>A <see cref="RefreshResponse"/> with new token and expiration if successful.</returns>
        public async Task<RefreshResponse> Refresh(RefreshRequest refreshTokenRequest)
        {
            var rtCleanup = CleanupRefreshTokensAsync();

            var encodedToken = EncodeRefreshToken(refreshTokenRequest.RefreshToken);
            // Get the user from the repository based on the username
            UserRefreshToken? userRefreshToken = _identityDbContext.UserRefreshTokens.FirstOrDefault(x => x.RefreshToken == encodedToken);
            if (userRefreshToken != null)
            {
                // Get the user from the repository based on the userId of the token
                var user = _identityDbContext.Users.FirstOrDefault(x => x.UserId == userRefreshToken.UserId);

                var tokenDescriptor = CreateClaimTokenForUser(user);
                var refreshToken = Guid.NewGuid().ToString();

                var tokenHandler = new JwtSecurityTokenHandler();
                var token = tokenHandler.CreateToken(tokenDescriptor);
                var stringToken = tokenHandler.WriteToken(token);

                await rtCleanup;

                _identityDbContext.UserRefreshTokens.Add(new UserRefreshToken() { UserId = user.UserId, RefreshToken = EncodeRefreshToken(refreshToken), ValidTo = DateTime.UtcNow.AddDays(1) });
                _identityDbContext.UserRefreshTokens.Remove(userRefreshToken);
                _identityDbContext.SaveChanges();

                return new RefreshResponse() { Token = stringToken, Expires = tokenDescriptor.Expires, RefreshToken = refreshToken };
            }

            await rtCleanup;
            return new RefreshResponse() { Token = null, Expires = null };
        }

        /// <summary>
        /// Creates a <see cref="SecurityTokenDescriptor"/> with claims for the specified user.
        /// </summary>
        /// <param name="user">The user for whom to create the token.</param>
        /// <returns>A configured <see cref="SecurityTokenDescriptor"/>.</returns>
        private SecurityTokenDescriptor CreateClaimTokenForUser(User? user)
        {
            var issuer = _config["Jwt:Issuer"];
            var audience = _config["Jwt:Audience"];
            var key = Encoding.ASCII.GetBytes(_config["Jwt:Key"]);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                        new Claim("Id", Guid.NewGuid().ToString()),
                        new Claim(ClaimTypes.Sid, user.UserId.ToString()),
                        new Claim(ClaimTypes.Name, user.UserName),
                        new Claim(JwtRegisteredClaimNames.Email, user.Email),
                        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
                    }, "password"),
                Expires = DateTime.UtcNow.Add(ExpirationTime),
                Issuer = issuer,
                Audience = audience,
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha512Signature),
            };

            return tokenDescriptor;
        }

        /// <summary>
        /// Encodes a refresh token using SHA-512 and Base64 for secure storage.
        /// </summary>
        /// <param name="refreshToken">The refresh token to encode.</param>
        /// <returns>The encoded refresh token string.</returns>
        private string EncodeRefreshToken(string refreshToken)
        {
            return Convert.ToBase64String(CryptoHelpers.Sha512(Encoding.UTF8.GetBytes(refreshToken)));
        }

        /// <summary>
        /// Removes expired refresh tokens from the database.
        /// </summary>
        private async Task CleanupRefreshTokensAsync()
        {
            _identityDbContext.UserRefreshTokens.Where(x => x.ValidTo < DateTime.UtcNow).ExecuteDelete();
        }

        /// <summary>
        /// Extracts user information and permissions from a claims principal.
        /// </summary>
        /// <param name="user">The claims principal representing the authenticated user.</param>
        /// <returns>A <see cref="UserInfo"/> object with user details and permissions.</returns>
        public UserInfo[] GetUserInfoFromClaim(ClaimsPrincipal user)
        {
            if (!(user?.Identity?.IsAuthenticated ?? false))
                return [new UserInfo() { IsAuthenticated = false }];

            var users = new List<UserInfo>();
            foreach (var identity in user.Identities)
            {
                var userInfo = new UserInfo
                {
                    UserId = Guid.Parse(identity.FindFirst(ClaimTypes.Sid).Value.Trim('\"')),
                    IsAuthenticated = identity.IsAuthenticated,
                    UserName = identity.Name,
                    Claims = identity.Claims.Where(t => !t.Type.StartsWith("oi_")).Select(c => new KeyValuePair<string,string>(c.Type, c.Value))
                };

                var dbUser = _identityDbContext.Users
                    .Include(t => t.SystemRoleAssignments).ThenInclude(t => t.Role).ThenInclude(t => t.Permissions).ThenInclude(t => t.ApprovedActions)
                    .Include(t => t.SystemRoleAssignments).ThenInclude(t => t.Role).ThenInclude(t => t.Permissions).ThenInclude(t => t.DeniedActions)
                    .Include(t => t.InvestigationRoleAssignments).ThenInclude(t => t.Role).ThenInclude(t => t.Permissions).ThenInclude(t => t.ApprovedActions)
                    .Include(t => t.InvestigationRoleAssignments).ThenInclude(t => t.Role).ThenInclude(t => t.Permissions).ThenInclude(t => t.DeniedActions)
                    .FirstOrDefault(t => t.UserId == userInfo.UserId);

                if (dbUser != null)
                {
                    var approvedSystemActions = dbUser.SystemRoleAssignments.SelectMany(t => t.Role.Permissions.SelectMany(t => t.ApprovedActions.Select(t => t.Name))).Distinct().ToList();
                    var deniedSystemActions = dbUser.SystemRoleAssignments.SelectMany(t => t.Role.Permissions.SelectMany(t => t.DeniedActions.Select(t => t.Name))).Distinct().ToList();

                    userInfo.SystemActions = approvedSystemActions.Except(deniedSystemActions).ToList();

                    var approvedInvestigationActions = dbUser.InvestigationRoleAssignments.Select(t => new { t.InvestigationId, t.Role }).GroupBy(t => t.InvestigationId).ToDictionary(t => t.Key, t => t.SelectMany(t => t.Role.Permissions.SelectMany(t => t.ApprovedActions.Select(t => t.Name))).Distinct());
                    var deniedInvestigationActions = dbUser.InvestigationRoleAssignments.Select(t => new { t.InvestigationId, t.Role }).GroupBy(t => t.InvestigationId).ToDictionary(t => t.Key, t => t.SelectMany(t => t.Role.Permissions.SelectMany(t => t.DeniedActions.Select(t => t.Name))).Distinct());

                    userInfo.InvestigationActions = approvedInvestigationActions.ToDictionary(t => t.Key, t => (ICollection<string>)t.Value.Except(deniedInvestigationActions[t.Key]).ToList());
                }

                users.Add(userInfo);
            }
            return users.ToArray();
        }

        /// <summary>
        /// Retrieves the <see cref="User"/> entity from the database based on the provided <see cref="UserInfo"/>.
        /// </summary>
        /// <param name="userInfo">The user info containing the user ID.</param>
        /// <returns>The <see cref="User"/> entity if found; otherwise, null.</returns>
        public User GetUserFromClaim(UserInfo[] userInfo)
        {
            foreach (var user in userInfo)
            {
                if (user.IsAuthenticated)
                {
                    return _identityDbContext.Users.FirstOrDefault(t => t.UserId == user.UserId);
                }
            }

            return null;
        }

        public bool CanSignIn(User user)
        {
            return true;
        }
    }
}
