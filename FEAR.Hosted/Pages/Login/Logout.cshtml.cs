using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FEAR.Hosted.Pages.Login
{
    /// <summary>
    /// Razor PageModel for handling user logout.
    /// Deletes the authentication cookie to sign the user out.
    /// </summary>
    public class LogoutModel : PageModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LoginModel"/> class.
        /// </summary>
        /// <param name="identityProvider">The identity provider for authentication operations.</param>
        public LogoutModel()
        {
        }

        /// <summary>
        /// Handles GET requests to log the user out by removing the authentication token cookie.
        /// </summary>
        public async Task OnPost()
        {
            var x = new System.Security.Claims.ClaimsPrincipal(User.Identity);
            await Response.HttpContext.SignOutAsync();
        }
    }
}