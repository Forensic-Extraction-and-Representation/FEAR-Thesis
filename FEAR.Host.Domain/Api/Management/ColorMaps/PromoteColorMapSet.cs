using FEAR.Domain.Model.ColorMap;

namespace FEAR.Host.Domain.Api.Management.ColorMaps
{
    public class PromoteColorMapSet
    {
        public class Request
        {
            public ColorMapSet ColorMapSet { get; set; } = null!;
        }
        public class Response
        {
            public Guid NewColorMapSetId { get; set; }
            public bool Success { get; set; }
        }
    }
}
