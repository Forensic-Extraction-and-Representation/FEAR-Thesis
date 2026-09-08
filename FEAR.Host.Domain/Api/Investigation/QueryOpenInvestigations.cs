using FEAR.Domain.Api;

namespace FEAR.Host.Domain.Api.Investigation
{
    public class QueryOpenInvestigations
    {
        /// <summary>
        /// Represents summary information about an open investigation.
        /// </summary>
        public class OpenInvestigation
        {
            /// <summary>
            /// The unique identifier of the investigation.
            /// </summary>
            public Guid InvestigationId { get; set; }

            /// <summary>
            /// The name of the investigation.
            /// </summary>
            public string Name { get; set; } = "";

            /// <summary>
            /// The case number associated with the investigation.
            /// </summary>
            public string CaseNumber { get; set; }
        }

        /// <summary>
        /// Represents the response for a query returning a list of open investigations.
        /// Each result contains minimal information about an open investigation.
        /// </summary>
        public class Response : BaseQueryResponse<OpenInvestigation>
        {
        }
    }
}
