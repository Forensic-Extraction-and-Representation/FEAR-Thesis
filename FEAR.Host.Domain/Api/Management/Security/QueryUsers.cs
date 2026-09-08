using FEAR.Domain.Api;

namespace FEAR.Host.Domain.Api.Management.Security
{
    /// <summary>
    /// Represents a request to query users in the system.
    /// Inherits pagination and filtering properties from <see cref="BaseQueryRequest"/>.
    /// </summary>
    public class QueryUsersRequest : BaseQueryRequest
    {
        /// <summary>
        /// An optional filter to search for users by username.
        /// </summary>
        public string UserNameFilter { get; set; } = string.Empty;
    }

    /// <summary>
    /// Represents the response for a user query operation.
    /// Inherits result and pagination properties from <see cref="BaseQueryResponse{User}"/>.
    /// </summary>
    public class QueryUsersResponse : BaseQueryResponse<FEAR.Domain.Model.Authentication.User>
    {
    }
}
