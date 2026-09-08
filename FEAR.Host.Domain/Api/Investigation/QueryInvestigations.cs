using FEAR.Domain.Api;

namespace FEAR.Host.Domain.Api.Investigation
{
    public class QueryInvestigations
    {
        /// <summary>
        /// Represents a request to query investigations.
        /// Inherits pagination and filtering properties from <see cref="BaseQueryRequest"/>.
        /// </summary>
        public class Request : BaseQueryRequest
        {
        }

        /// <summary>
        /// Represents the response for a query returning a list of investigations.
        /// Inherits result and pagination properties from <see cref="BaseQueryResponse{InvestigationInfo}"/>.
        /// </summary>
        public class Response : BaseQueryResponse<FEAR.Domain.Model.InvestigationStore.InvestigationInfo>
        {
        }
    }
}
