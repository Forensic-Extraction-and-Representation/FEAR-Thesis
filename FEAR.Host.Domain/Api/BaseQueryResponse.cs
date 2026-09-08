namespace FEAR.Domain.Api
{
    /// <summary>
    /// Base class for paginated query responses.
    /// Provides common properties for returning results and pagination metadata.
    /// </summary>
    /// <typeparam name="T">The type of result returned in the query.</typeparam>
    public abstract class BaseQueryResponse<T>
    {
        /// <summary>
        /// The collection of results for the current page.
        /// </summary>
        public IEnumerable<T> Results { get; set; }

        /// <summary>
        /// The total number of results available for the query.
        /// </summary>
        public int TotalResults { get; set; }

        /// <summary>
        /// The number of results returned in this page.
        /// </summary>
        public int ReturnedResults { get; set; }

        /// <summary>
        /// The current page number of the results.
        /// </summary>
        public int Page { get; set; }
    }
}
