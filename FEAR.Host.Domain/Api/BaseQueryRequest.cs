namespace FEAR.Domain.Api
{
    /// <summary>
    /// Base class for paginated query requests.
    /// Provides common properties for specifying page size and page number.
    /// </summary>
    public abstract class BaseQueryRequest
    {
        /// <summary>
        /// The maximum number of results to return per page. Default is 100.
        /// </summary>
        public int MaxPerPage { get; set; } = 100;

        /// <summary>
        /// The page number to retrieve. Default is 1.
        /// </summary>
        public int Page { get; set; } = 1;
    }
}
