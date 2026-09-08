using FEAR.Host.Core.Identity;
using FEAR.Host.Domain.Api.Authentication;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OpenIddict.Validation.AspNetCore;
using System.ComponentModel.DataAnnotations;

namespace FEAR.Hosted.Pages.Login
{
    /// <summary>
    /// Razor PageModel for handling user login.
    /// Manages user input, validation, authentication, and error feedback for the login page.
    /// </summary>
    public class LoginModel : PageModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LoginModel"/> class.
        /// </summary>
        /// <param name="identityProvider">The identity provider for authentication operations.</param>
        public LoginModel(IIdentityProvider identityProvider, IConfiguration configuration)
        {
            Configuration = configuration;
            IdentityProvider = identityProvider;
        }

        /// <summary>
        /// The username entered by the user.
        /// </summary>
        [BindProperty]
        [Required]
        public string Username { get; set; }

        /// <summary>
        /// The password entered by the user.
        /// </summary>
        [BindProperty]
        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        /// <summary>
        /// Error message to display if login fails.
        /// </summary>
        public string ErrorMessage { get; set; }

        /// <summary>
        /// The identity provider used for authentication.
        /// </summary>
        private IIdentityProvider IdentityProvider { get; set; }

        /// <summary>
        /// The configuration provider of the web application.
        /// </summary>
        private IConfiguration Configuration { get; set; }

        /// <summary>
        /// Handles GET requests to display the login page.
        /// </summary>
        public void OnGet()
        {
            // Show login page
        }

        /// <summary>
        /// Handles POST requests to authenticate the user.
        /// Validates input, attempts login, and sets authentication cookie if successful.
        /// </summary>
        /// <returns>A redirect to the return URL or home page if successful; otherwise, redisplays the login page with an error.</returns>
        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var loginResponse = await IdentityProvider.Login(new LoginRequest
            {
                Username = Username,
                Password = Password
            });

            // Check authentication result and set cookie if successful
            if (loginResponse.IsSuccess)
            {
                var x = new System.Security.Claims.ClaimsPrincipal(loginResponse.UserIdentity);
                await Response.HttpContext.SignInAsync(x);
                
                // Redirect to returnUrl if provided, otherwise to home page
                var returnUrl = Request.Query["returnUrl"].ToString();
                if (!string.IsNullOrEmpty(returnUrl))
                {
                    return LocalRedirect(returnUrl);
                }

                return RedirectToPage("/Index");
            }

            ErrorMessage = "Invalid username or password.";
            return Page();
        }
    }
}
