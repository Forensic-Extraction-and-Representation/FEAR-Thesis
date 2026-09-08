using FEAR.Host.Core;
using FEAR.Host.Core.Database;
using FEAR.Host.Core.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FEAR.Hosted.Pages.Cases
{
    /// <summary>
    /// Razor PageModel for creating a new investigation case.
    /// Handles form input, validation, and persistence of new case data to the database.
    /// Inherits authentication and user context from <see cref="AuthenticatedPageModel"/>.
    /// </summary>
    public class CreateModel : AuthenticatedPageModel
    {
        /// <summary>
        /// Database context for managing investigations.
        /// </summary>
        private readonly InvestigationManagementDbContext _investigationManagementDbContext;

        /// <summary>
        /// Represents the input fields for creating a new case.
        /// </summary>
        public class CaseInput
        {
            /// <summary>
            /// The name of the case.
            /// </summary>
            public string Name { get; set; } = string.Empty;

            /// <summary>
            /// The case number for the investigation.
            /// </summary>
            public string CaseNumber { get; internal set; } = string.Empty;

            /// <summary>
            /// The description of the case.
            /// </summary>
            public string Description { get; internal set; } = string.Empty;
        }

        /// <summary>
        /// The input model bound to the form for creating a new case.
        /// </summary>
        [BindProperty]
        public CaseInput Input { get; set; } = new CaseInput();

        /// <summary>
        /// Initializes a new instance of the <see cref="CreateModel"/> class.
        /// </summary>
        /// <param name="investigationManagementDbContext">The database context for investigations.</param>
        /// <param name="identityProvider">The identity provider for user resolution.</param>
        public CreateModel(InvestigationManagementDbContext investigationManagementDbContext, IIdentityProvider identityProvider)
            : base(identityProvider)
        {
            _investigationManagementDbContext = investigationManagementDbContext;
        }

        /// <summary>
        /// Handles GET requests to display the create case form.
        /// </summary>
        public void OnGet()
        {
        }

        /// <summary>
        /// Handles POST requests to create a new investigation case.
        /// Validates the input, creates the case entity, and saves it to the database.
        /// Requires the "System.Investigation.Create" system action permission.
        /// </summary>
        /// <returns>A redirect to the index page if successful; otherwise, redisplays the form.</returns>
        [SystemActionAuthorize("System.Investigation.Create")]
        public IActionResult OnPost()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var txn = _investigationManagementDbContext.Database.BeginTransaction();
            FEAR.Domain.Dto.InvestigationStore.InvestigationInfo Case = new FEAR.Domain.Dto.InvestigationStore.InvestigationInfo();
            Case.InvestigationId = Guid.NewGuid();
            Case.CreatedByUserId = CurrentUserInfo.Value.First().UserId;
            Case.CreatedOn = DateTime.UtcNow;
            Case.LastModifiedByUserId = CurrentUserInfo.Value.First().UserId;
            Case.LastModifiedOn = DateTime.UtcNow;
            Case.Name = Input.Name;
            Case.Description = Input.Description;
            Case.CaseDate = DateTime.UtcNow;
            Case.CaseStatus = "Open";
            Case.CaseType = "Investigation";
            Case.CaseNumber = Input.CaseNumber;
            Case.Namespace= "https://example.com";
            Case.NamespaceAbbrev = "EX";
            _investigationManagementDbContext.Investigations.Add(Case);
            _investigationManagementDbContext.SaveChanges();
            txn.Commit();
            return RedirectToPage("Index");
        }
    }
}
