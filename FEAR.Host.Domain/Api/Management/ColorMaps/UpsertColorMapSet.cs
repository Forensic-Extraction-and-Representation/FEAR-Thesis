using FEAR.Domain.Model.ColorMap;

namespace FEAR.Host.Domain.Api.Management.ColorMaps
{
    public class UpsertColorMapSet
    {
        public class Request
        {
            public ColorMapSet Set { get; set; }
        }
        public class Response
        {
            public Guid ColorMapSetId { get; set; }
            public bool Success { get; set; }
        }
    }
}
