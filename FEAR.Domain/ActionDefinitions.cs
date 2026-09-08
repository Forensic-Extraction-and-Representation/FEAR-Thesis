using FEAR.Domain.Dto.Authentication;

namespace FEAR.Domain
{
    /// <summary>
    /// Provides a centralized definition of all system and investigation actions for permission and authorization checks.
    /// Each action is represented as a <see cref="SystemAction"/> and grouped by logical area (e.g., User, Role, Investigation).
    /// These definitions are used throughout the system to control access, assign permissions, and enforce security policies.
    /// </summary>
    public static partial class ActionDefinitions
    {
    }
}
