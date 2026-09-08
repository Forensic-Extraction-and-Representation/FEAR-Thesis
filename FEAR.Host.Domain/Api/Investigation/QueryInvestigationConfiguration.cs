using FEAR.Domain.Model;

namespace FEAR.Host.Domain.Api.Investigation
{
    public class QueryInvestigationConfiguration
    {

        /// <summary>
        /// Represents a request to retrieve the configuration for a specific investigation.
        /// </summary>
        public class Request
        {
            /// <summary>
            /// The unique identifier of the investigation whose configuration is being requested.
            /// </summary>
            public Guid InvestigationId { get; set; }
        }

        /// <summary>
        /// Represents the response containing the configuration data for an investigation.
        /// </summary>
        public class Response : Request
        {
            /// <summary>
            /// The JSON serialized configuration data for the investigation.
            /// </summary>
            public CaseConfiguration Configuration { get; set; }
        }
    }
}
