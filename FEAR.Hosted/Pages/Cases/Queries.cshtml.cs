using FEAR.Domain.Dto.InvestigationStore;
using FEAR.Host.Core;
using FEAR.Host.Core.Database;
using FEAR.Host.Core.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FEAR.Hosted.Pages.Cases
{
    /// <summary>
    /// Razor PageModel for managing and displaying investigation queries for a specific case.
    /// Allows users with the appropriate role to view and add shared queries for an investigation.
    /// Inherits authentication and user context from <see cref="AuthenticatedPageModel"/>.
    /// </summary>
    [InvestigationActionAuthorize("Investigation.Role.Update", "investigationId")]
    public class QueriesModel : AuthenticatedPageModel
    {
        /// <summary>
        /// Database context for accessing and managing investigation queries.
        /// </summary>
        private readonly InvestigationManagementDbContext _investigationManagementDbContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="QueriesModel"/> class.
        /// </summary>
        /// <param name="investigationManagementDbContext">The database context for investigations and queries.</param>
        /// <param name="identityProvider">The identity provider for user resolution.</param>
        public QueriesModel(InvestigationManagementDbContext investigationManagementDbContext, IIdentityProvider identityProvider)
            : base(identityProvider)
        {
            _investigationManagementDbContext = investigationManagementDbContext;
        }

        /// <summary>
        /// The unique identifier of the investigation for which queries are managed.
        /// </summary>
        [BindProperty(SupportsGet = true)]
        public Guid InvestigationId { get; set; }

        /// <summary>
        /// The new query to be added to the investigation.
        /// </summary>
        [BindProperty]
        public InvestigationQuery NewQuery { get; set; }

        /// <summary>
        /// The list of queries associated with the current investigation.
        /// </summary>
        public List<InvestigationQuery> Queries { get; set; }

        /// <summary>
        /// Handles GET requests to load and display all queries for the specified investigation.
        /// </summary>
        public async Task OnGetAsync()
        {
            Queries = await _investigationManagementDbContext.InvestigationQuerys
                .Where(q => q.InvestigationId == InvestigationId)
                .OrderBy(q => q.CreatedOn)
                .ToListAsync();
        }

        /// <summary>
        /// Handles POST requests to add a new query to the investigation.
        /// Validates the input, sets metadata, and saves the new query to the database.
        /// </summary>
        /// <returns>A redirect to the same page with the current investigation ID if successful; otherwise, redisplays the form.</returns>
        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                await OnGetAsync(); // reload the list for redisplay
                return Page();
            }

            NewQuery.CreatedByUserId = CurrentUserInfo.Value.First().UserId;
            NewQuery.CreatedOn = DateTime.UtcNow;
            NewQuery.InvestigationId = InvestigationId;

            _investigationManagementDbContext.InvestigationQuerys.Add(NewQuery);
            await _investigationManagementDbContext.SaveChangesAsync();

            return RedirectToPage(new { investigationId = InvestigationId });
        }
    }
}
