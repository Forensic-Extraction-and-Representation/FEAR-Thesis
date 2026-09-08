namespace FEAR.Host.Domain.Api.Management.Security
{
    public class DeletePermissionSet
    {
        public class Request
        {
            public Guid PermissionSetId { get; set; }
        }
        public class Response
        {
            public bool Success { get; set; }
        }
    }
}
