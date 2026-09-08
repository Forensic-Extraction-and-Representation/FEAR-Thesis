namespace FEAR.Host.Domain.Api.Management.Security
{
    public class UpdatePermissionSet
    {
        public class Request
        {
            public Guid PermissionSetId { get; set; }
            public string Name { get; set; } = string.Empty;
            public List<Guid> ApprovedActionIds { get; set; } = new();
            public List<Guid> DeniedActionIds { get; set; } = new();
        }
        public class Response
        {
            public bool Success { get; set; }
        }
    }
}
