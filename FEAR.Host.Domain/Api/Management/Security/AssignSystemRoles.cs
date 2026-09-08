namespace FEAR.Host.Domain.Api.Management.Security
{
    public abstract class BaseSystemRoles
    {
        public class Request
        {
            public Guid UserId { get; set; }
            public List<Guid> RoleIds { get; set; } = new();
        }
        public class Response
        {
            public bool Success { get; set; }
        }
    }

    public class AssignSystemRoles : BaseSystemRoles
    {
        public AssignSystemRoles() { }
    }
     public class RevokeSystemRoles : BaseSystemRoles
    {
        public RevokeSystemRoles() { }
    }
}
