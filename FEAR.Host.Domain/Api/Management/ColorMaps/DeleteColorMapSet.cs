namespace FEAR.Host.Domain.Api.Management.ColorMaps
{
    public class DeleteColorMapSet
    {
        public class Request
        {
            public Guid ColorMapSetId { get; set; }
        }
        public class Response
        {
            public bool Success { get; set; }
        }
    }
}
