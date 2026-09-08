using FEAR.Domain.Dto.Authentication;

namespace FEAR.Host.Core.IdentitySeed
{
    /// <summary>
    /// Provides predefined permission sets for system and investigation administrators.
    /// These sets group approved actions for assignment to roles.
    /// </summary>
    public class PermissionSets
    {
        private readonly SystemActionSets systemActionSets = new SystemActionSets();

        /// <summary>
        /// Gets a permission set granting all system-level actions.
        /// </summary>
        /// <returns>A <see cref="PermissionSet"/> with all actions starting with "System" approved.</returns>
        public PermissionSet SystemAdminPermissionSet()
        {
            var p = new PermissionSet
            {
                Name = "All-System",
                ApprovedActions = systemActionSets.Actions.Where(a => a.Name.StartsWith("System")).ToList()
            };

            return p;
        }

        /// <summary>
        /// Gets a permission set granting all investigation-level actions.
        /// </summary>
        /// <returns>A <see cref="PermissionSet"/> with all actions starting with "Investigation" approved.</returns>
        public PermissionSet InvestigationAdminPermissionSet()
        {
            var p = new PermissionSet
            {
                Name = "All-Investigation",
                ApprovedActions = systemActionSets.Actions.Where(a => a.Name.StartsWith("Investigation")).ToList()
            };

            return p;
        }
    }
}
