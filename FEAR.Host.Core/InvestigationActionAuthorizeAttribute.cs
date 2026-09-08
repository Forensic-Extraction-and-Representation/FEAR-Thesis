using Microsoft.AspNetCore.Mvc;

namespace FEAR.Host.Core
{
    /// <summary>
    /// Attribute for authorizing actions on a case or entity based on user permissions.
    /// Uses <see cref="InvestigationActionAuthorizeFilter"/> to enforce authorization logic.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
    public class InvestigationActionAuthorizeAttribute : TypeFilterAttribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="InvestigationActionAuthorizeAttribute"/> class.
        /// </summary>
        /// <param name="action">The action name to authorize (e.g., "View", "Edit").</param>
        /// <param name="entityIdRouteParam">The route parameter name that contains the entity or case ID.</param>
        public InvestigationActionAuthorizeAttribute(string action, string entityIdRouteParam)
            : base(typeof(InvestigationActionAuthorizeFilter))
        {
            Arguments = new object[] { action, entityIdRouteParam };
        }
    }
}