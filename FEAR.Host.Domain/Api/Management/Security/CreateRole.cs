namespace FEAR.Host.Domain.Api.Management.Security
{
    public class CreateRole
    {
        public class Request
        {
            public string Name { get; set; } = string.Empty;
            public List<Guid> PermissionSetIds { get; set; } = new();
        }
        public class Response
        {
            public Guid RoleId { get; set; }
            public bool Success { get; set; }
        }
    }
}
