using Microsoft.AspNetCore.Mvc;

namespace FEAR.Host.Core
{
    /// <summary>
    /// Attribute for authorizing system-level actions based on user permissions.
    /// Uses <see cref="SystemeActionAuthorizeFilter"/> to enforce authorization logic.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
    public class SystemActionAuthorizeAttribute : TypeFilterAttribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SystemActionAuthorizeAttribute"/> class.
        /// </summary>
        /// <param name="action">The system action name to authorize (e.g., "System.View", "System.Edit").</param>
        public SystemActionAuthorizeAttribute(string action)
            : base(typeof(SystemeActionAuthorizeFilter))
        {
            Arguments = new object[] { action };
        }
    }
}