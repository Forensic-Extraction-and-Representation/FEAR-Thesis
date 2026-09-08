using FEAR.Domain.Api;

namespace FEAR.Host.Domain.Api.Management.Security
{
    public class QuerySystemUserRoles
    {
        public class Request : BaseQueryRequest
        {
            public Guid UserId { get; set; }
        }
        public class SystemUserRoles
        {
            public Guid UserId { get; set; }
            public string UserName { get; set; } = string.Empty;
            public string Email { get; set; } = string.Empty;
            public List<Guid> RoleIds { get; set; } = new();
            public List<string> RoleNames { get; set; } = new();
        }
        public class Response : BaseQueryResponse<SystemUserRoles>
        {
        }
    }
}
