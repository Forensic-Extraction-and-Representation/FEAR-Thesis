using FEAR.Blazor.Shared.Services;
using FEAR.Host.Core.Api.Management.Security;
using FEAR.Host.Domain.Api.Management.Security;

namespace FEAR.Admin.Services.Management
{
    public class UserService : BaseService
    {
        public UserService(HttpFactory httpClientFactory): base(httpClientFactory)
        {
        }

        public Task<QueryUsersResponse> QueryUsersAsync(QueryUsersRequest request)
            => PostAsync<QueryUsersRequest, QueryUsersResponse>("v1.0/Management/User/QueryUsers", request);

        public Task<CreateUserResponse> CreateUserAsync(CreateUserRequest request)
            => PostAsync<CreateUserRequest, CreateUserResponse>("v1.0/Management/User/CreateUser", request);

        public Task<UpdateUserResponse> UpdateUserAsync(UpdateUserRequest request)
            => PostAsync<UpdateUserRequest, UpdateUserResponse>("v1.0/Management/User/UpdateUser", request);

        public Task<QuerySystemUserRoles.Response> QueryUserSystemRolesAsync(QuerySystemUserRoles.Request request)
            => PostAsync<QuerySystemUserRoles.Request, QuerySystemUserRoles.Response>("v1.0/Management/User/QuerySystemUserRoles", request);

        public Task<UpdatePasswordResponse> UpdatePasswordAsync(UpdatePasswordRequest request)
            => PostAsync<UpdatePasswordRequest, UpdatePasswordResponse>("v1.0/Management/User/UpdatePassword", request);

        public Task<QueryRoles.Response> QueryRolesAsync(QueryRoles.Request request)
            => PostAsync<QueryRoles.Request, QueryRoles.Response>("v1.0/Management/User/QueryRoles", request);

        public Task<AssignSystemRoles.Response> AssignSystemRolesAsync(AssignSystemRoles.Request request)
            => PostAsync<AssignSystemRoles.Request, AssignSystemRoles.Response>("v1.0/Management/User/AssignSystemRoles", request);

        public Task<RevokeSystemRoles.Response> RevokeSystemRolesAsync(RevokeSystemRoles.Request request)
            => PostAsync<RevokeSystemRoles.Request, RevokeSystemRoles.Response>("v1.0/Management/User/RevokeSystemRoles", request);

        public Task<AssignInvestigationRoles.Response> AssignInvestigationRolesAsync(AssignInvestigationRoles.Request request)
            => PostAsync<AssignInvestigationRoles.Request, AssignInvestigationRoles.Response>("v1.0/Management/User/AssignInvestigationRoles", request);

        public Task<QueryInvestigationUserRoles.Response> QueryInvestigationUsersAsync(QueryInvestigationUserRoles.Request request)
            => PostAsync<QueryInvestigationUserRoles.Request, QueryInvestigationUserRoles.Response>("v1.0/Management/User/QueryInvestigationUserRoles", request);

        public Task<RevokeInvestigationRoles.Response> RevokeInvestigationRolesAsync(RevokeInvestigationRoles.Request request)
            => PostAsync<RevokeInvestigationRoles.Request, RevokeInvestigationRoles.Response>("v1.0/Management/User/RevokeInvestigationRoles", request);
    }
}
