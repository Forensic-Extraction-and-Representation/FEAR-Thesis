using FEAR.Host.Core;
using FEAR.Host.Core.Database;
using FEAR.Host.Core.Identity;
using Microsoft.AspNetCore.Mvc;

namespace FEAR.Hosted.Pages.Cases
{
    /// <summary>
    /// Razor PageModel for configuring an investigation's JSON configuration file.
    /// Requires the user to have the "System.Investigation.Update" system action permission.
    /// </summary>
    [SystemActionAuthorize("System.Investigation.Update")]
    public class ConfigureModel : AuthenticatedPageModel
    {
        /// <summary>
        /// Database context for managing investigations and configurations.
        /// </summary>
        private readonly InvestigationManagementDbContext _investigationManagementDbContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigureModel"/> class.
        /// </summary>
        /// <param name="investigationManagementDbContext">The database context for investigations.</param>
        /// <param name="identityProvider">The identity provider for user resolution.</param>
        public ConfigureModel(InvestigationManagementDbContext investigationManagementDbContext, IIdentityProvider identityProvider)
            : base(identityProvider)
        {
            _investigationManagementDbContext = investigationManagementDbContext;
        }

        /// <summary>
        /// The file path to the JSON configuration file being edited.
        /// </summary>
        /// TODO: Change this to be based on the investigation ID from the management database context.
        public string JsonFilePath = "C:\\FEAR\\Autopsy\\Configuration\\test.json";

        /// <summary>
        /// The contents of the JSON configuration file.
        /// This could be set from the investigation management database context based on the investigation ID.
        /// </summary>
        public string JsonContent = "";

        /// <summary>
        /// The unique identifier of the investigation being configured.
        /// </summary>
        [BindProperty(SupportsGet = true)]
        public Guid InvestigationId { get; set; }

        /// <summary>
        /// Handles GET requests to load the JSON configuration file content.
        /// </summary>
        public void OnGet()
        {
            JsonContent = System.IO.File.Exists(JsonFilePath) ? System.IO.File.ReadAllText(JsonFilePath) : "{}";
        }

        /// <summary>
        /// Handles POST requests to update the JSON configuration file.
        /// Validates the JSON and writes it to disk if valid.
        /// </summary>
        /// <returns>A redirect to the same page with the current investigation ID.</returns>
        public async Task<IActionResult> OnPostAsync() 
        {
            string updatedJson = Request.Form["jsonEditor"];
            try
            {
                // Validate JSON format
                Newtonsoft.Json.Linq.JToken.Parse(updatedJson);
                System.IO.File.WriteAllText(JsonFilePath, updatedJson);
            }
            catch (Exception ex)
            {
            }

            return RedirectToPage(new { investigationId = InvestigationId });
        }
    }
}
