using FEAR.Domain;
using FEAR.Domain.Dto.Authentication;
using MassTransit;

namespace FEAR.Host.Core.IdentitySeed
{
    /// <summary>
    /// Provides collections and utilities for system and investigation actions used in permission and authorization checks.
    /// Aggregates all defined actions and supports creation and lookup of actions by name.
    /// </summary>
    public class SystemActionSets
    {
        /// <summary>
        /// All system-level actions defined in the application.
        /// </summary>
        public static readonly ICollection<SystemAction> AllSystemActions = ActionDefinitions.System.All;

        /// <summary>
        /// All investigation-level actions defined in the application.
        /// </summary>
        public static readonly ICollection<SystemAction> AllInvestigationActions = ActionDefinitions.Investigation.All;

        /// <summary>
        /// Creates a list of <see cref="SystemAction"/> objects with names based on a prefix and an array of action names.
        /// </summary>
        /// <param name="name">The prefix for the action names (e.g., "System.PermissionSet").</param>
        /// <param name="actions">The array of action name suffixes (e.g., "Create", "List").</param>
        /// <returns>A collection of <see cref="SystemAction"/> objects with generated names.</returns>
        public static ICollection<SystemAction> CreateActionList(string name, string[] actions)
        {
            var l = new List<SystemAction>();

            foreach (var a in actions)
            {
                l.Add(CreateAction($"{name}.{a}"));
            }

            return l;
        }

        /// <summary>
        /// Creates a new <see cref="SystemAction"/> with a unique identifier and the specified name.
        /// </summary>
        /// <param name="name">The name of the system action.</param>
        /// <returns>A new <see cref="SystemAction"/> instance.</returns>
        private static SystemAction CreateAction(string name)
        {
            var sa = new SystemAction
            {
                SystemActionId = NewId.NextSequentialGuid(),
                Name = name
            };

            return sa;
        }

        /// <summary>
        /// Finds a <see cref="SystemAction"/> by its name from the aggregated actions.
        /// </summary>
        /// <param name="name">The name of the action to find.</param>
        /// <returns>The matching <see cref="SystemAction"/>.</returns>
        public SystemAction ActionFor(string name)
        {
            return Actions.First(a => a.Name == name);
        }

        /// <summary>
        /// Gets a collection of all system and investigation actions available in the application.
        /// </summary>
        public ICollection<SystemAction> Actions => new List<SystemAction>()
            .Union(AllSystemActions)
            .Union(AllInvestigationActions)
            .ToList();
    }
}
