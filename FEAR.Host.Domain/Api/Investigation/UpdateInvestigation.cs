using FEAR.Domain.Api;
using FEAR.Domain.Model;
using FEAR.Domain.Model.InvestigationStore;

namespace FEAR.Host.Domain.Api.Investigation
{
    public class UpdateInvestigation
    {
        public class Request : InvestigationInfo
        {
            /// <summary>
            /// The configuration data (such as JSON or serialized settings) for the investigation.
            /// </summary>
            public CaseConfiguration Configuration { get; set; }
        }

        public class Response : BaseResponse
        {
            public Response()
            {
            }

            public Guid InvestigationId { get; set; }
        }
    }
}
