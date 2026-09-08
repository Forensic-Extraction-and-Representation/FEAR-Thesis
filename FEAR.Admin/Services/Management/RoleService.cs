using FEAR.Blazor.Shared.Services;
using FEAR.Host.Domain.Api.Management.Security;

namespace FEAR.Admin.Services.Management
{
    public class RoleService : BaseService
    {
        public RoleService(HttpFactory httpFactory) : base(httpFactory) { }

        public Task<QueryRoles.Response> QueryRolesAsync(QueryRoles.Request request)
            => PostAsync<QueryRoles.Request, QueryRoles.Response>("v1.0/Management/Role/QueryRoles", request);

        public Task<CreateRole.Response> CreateRoleAsync(CreateRole.Request request)
            => PostAsync<CreateRole.Request, CreateRole.Response>("v1.0/Management/Role/CreateRole", request);

        public Task<UpdateRole.Response> UpdateRoleAsync(UpdateRole.Request request)
            => PostAsync<UpdateRole.Request, UpdateRole.Response>("v1.0/Management/Role/UpdateRole", request);

        public Task<DeleteRole.Response> DeleteRoleAsync(DeleteRole.Request request)
            => PostAsync<DeleteRole.Request, DeleteRole.Response>("v1.0/Management/Role/DeleteRole", request);

        public Task<QueryPermissionSets.Response> QueryPermissionSetsAsync(QueryPermissionSets.Request request)
            => PostAsync<QueryPermissionSets.Request, QueryPermissionSets.Response>("v1.0/Management/Role/QueryPermissionSets", request);

        public Task<CreatePermissionSet.Response> CreatePermissionSetAsync(CreatePermissionSet.Request request)
            => PostAsync<CreatePermissionSet.Request, CreatePermissionSet.Response>("v1.0/Management/Role/CreatePermissionSet", request);

        public Task<UpdatePermissionSet.Response> UpdatePermissionSetAsync(UpdatePermissionSet.Request request)
            => PostAsync<UpdatePermissionSet.Request, UpdatePermissionSet.Response>("v1.0/Management/Role/UpdatePermissionSet", request);

        public Task<DeletePermissionSet.Response> DeletePermissionSetAsync(DeletePermissionSet.Request request)
            => PostAsync<DeletePermissionSet.Request, DeletePermissionSet.Response>("v1.0/Management/Role/DeletePermissionSet", request);

        public Task<QuerySystemActions.Response> QuerySystemActionsAsync(QuerySystemActions.Request request)
            => PostAsync<QuerySystemActions.Request, QuerySystemActions.Response>("v1.0/Management/Role/QuerySystemActions", request);
    }
}
