using FEAR.Host.Domain.Api.Authentication;
using FEAR.Domain.Dto.Authentication;
using FEAR.Host.Core.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FEAR.Hosted.Pages
{
    /// <summary>
    /// Base class for Razor PageModels that require an authenticated user.
    /// Provides convenient access to the current user's information and domain user entity
    /// by resolving them from the claims principal using the provided identity provider.
    /// </summary>
    public abstract class AuthenticatedPageModel : PageModel
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
        /// Initializes a new instance of the <see cref="AuthenticatedPageModel"/> class.
        /// </summary>
        /// <param name="identityProvider">The identity provider for user resolution.</param>
        public AuthenticatedPageModel(IIdentityProvider identityProvider)
        {
            _identityProvider = identityProvider;
            CurrentUserInfo = new Lazy<UserInfo[]>(() => _identityProvider.GetUserInfoFromClaim(User));
            CurrentUser = new Lazy<User>(() => _identityProvider.GetUserFromClaim(CurrentUserInfo.Value));
        }
    }
}
