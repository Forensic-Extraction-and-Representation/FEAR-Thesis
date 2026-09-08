using FEAR.Domain.Api;

namespace FEAR.Host.Domain.Api.Management.Security
{
    public class QueryInvestigationUserRoles
    {
        public class Request : BaseQueryRequest
        {
            public Guid InvestigationId { get; set; }
        }

        public class InvestigationUserRoles
        {
            public string UserName { get; set; }
            public Guid RoleId { get; set; }
            public string RoleName { get; set; } = string.Empty;
            public Guid InvestigationId { get; set; }
            public Guid UserId { get; set; }
        }

        public class Response : BaseQueryResponse<InvestigationUserRoles>
        {
            public Guid InvestigationId { get; set; }
        }
    }
}
