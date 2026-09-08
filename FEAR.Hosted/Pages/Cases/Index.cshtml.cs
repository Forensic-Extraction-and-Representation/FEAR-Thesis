using FEAR.Host.Core;
using FEAR.Host.Core.Database;
using FEAR.Host.Core.Identity;

namespace FEAR.Hosted.Pages.Cases
{
    /// <summary>
    /// Razor PageModel for listing all investigation cases.
    /// Requires the "System.Investigation.List" system action permission.
    /// Retrieves and exposes a list of investigations for display in the UI.
    /// Inherits authentication and user context from <see cref="AuthenticatedPageModel"/>.
    /// </summary>
    [SystemActionAuthorize("System.Investigation.List")]
    public class IndexModel : AuthenticatedPageModel
    {
        /// <summary>
        /// Database context for accessing investigations.
        /// </summary>
        private readonly InvestigationManagementDbContext _investigationManagementDbContext;

        /// <summary>
        /// The list of investigation cases to be displayed.
        /// </summary>
        public List<FEAR.Domain.Dto.InvestigationStore.InvestigationInfo> Cases { get; set; } = new List<FEAR.Domain.Dto.InvestigationStore.InvestigationInfo>();

        /// <summary>
        /// Initializes a new instance of the <see cref="IndexModel"/> class.
        /// </summary>
        /// <param name="investigationManagementDbContext">The database context for investigations.</param>
        /// <param name="identityProvider">The identity provider for user resolution.</param>
        public IndexModel(InvestigationManagementDbContext investigationManagementDbContext, IIdentityProvider identityProvider) : base(identityProvider)
        {
            _investigationManagementDbContext = investigationManagementDbContext;
        }

        /// <summary>
        /// Handles GET requests to retrieve and populate the list of investigation cases.
        /// </summary>
        public void OnGet()
        {
            Cases = _investigationManagementDbContext.Investigations.ToList();
        }
    }
}
