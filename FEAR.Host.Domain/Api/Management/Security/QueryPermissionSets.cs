using FEAR.Domain.Api;
using FEAR.Domain.Model.Authentication;

namespace FEAR.Host.Domain.Api.Management.Security
{
    public class QueryPermissionSets
    {
        public class Request : BaseQueryRequest
        {
            public string? NameFilter { get; set; }
        }

        public class Response : BaseQueryResponse<PermissionSet>
        {
        }
    }
}
