namespace FEAR.Host.Domain.Api.Management.Security
{
    public class DeleteRole
    {
        public class Request
        {
            public Guid RoleId { get; set; }
        }
        public class Response
        {
            public bool Success { get; set; }
        }
    }
}
