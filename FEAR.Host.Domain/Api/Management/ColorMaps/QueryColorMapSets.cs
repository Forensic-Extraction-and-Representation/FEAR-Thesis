using FEAR.Domain.Api;
using FEAR.Domain.Model.ColorMap;

namespace FEAR.Host.Domain.Api.Management.ColorMaps
{
    public class QueryColorMapSets
    {
        public class Request : BaseQueryRequest
        {
            public ColorMapEntityDiscriminator Scope { get; set; }
            public Guid? EntityId { get; set; }
            public string? NameFilter { get; set; }
        }

        public class Response : BaseQueryResponse<ColorMapSet>
        {
        }
    }
}
