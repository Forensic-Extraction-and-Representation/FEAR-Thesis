namespace FEAR.Host.Domain.Api.Management.Security
{
    public abstract class BaseInvestigationRoles
    {
        public class Request
        {
            public Guid UserId { get; set; }
            public Guid InvestigationId { get; set; }
            public Guid RoleId { get; set; }
        }
        public class Response
        {
            public bool Success { get; set; }
        }
    }

    public class AssignInvestigationRoles : BaseInvestigationRoles
    {
        public AssignInvestigationRoles() { }
    }

    public class RevokeInvestigationRoles : BaseInvestigationRoles
    {
        public RevokeInvestigationRoles() { }
    }
}
