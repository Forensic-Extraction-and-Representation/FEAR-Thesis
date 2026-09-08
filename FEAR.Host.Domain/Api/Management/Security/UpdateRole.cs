namespace FEAR.Host.Domain.Api.Management.Security
{
    public class UpdateRole
    {
        public class Request
        {
            public Guid RoleId { get; set; }
            public string Name { get; set; } = string.Empty;
            public List<Guid> PermissionSetIds { get; set; } = new();
        }
        public class Response
        {
            public bool Success { get; set; }
        }
    }
}
