using FEAR.Domain.Model.InvestigationStore;
using FEAR.Domain.Model;

namespace FEAR.Host.Domain.Api.Investigation
{
    public class CreateInvestigation
    {
        /// <summary>
        /// Represents a request to create a new investigation.
        /// Inherits investigation summary and metadata from <see cref="InvestigationInfo"/>,
        /// and includes additional configuration details for the investigation.
        /// </summary>
        public class Request : InvestigationInfo
        {
            /// <summary>
            /// The configuration data (such as JSON or serialized settings) for the investigation.
            /// </summary>
            public CaseConfiguration Configuration { get; set; }
        }

        /// <summary>
        /// Represents the response returned after successfully creating an investigation.
        /// Contains the unique identifier of the newly created investigation.
        /// </summary>
        public class Response : Request
        {
        }
    }
}
