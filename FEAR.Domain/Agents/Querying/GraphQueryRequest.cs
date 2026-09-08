namespace FEAR.Domain.Agents.Querying
{
    public class GraphQueryRequest
    {
        /// <summary>
        /// Gets or sets the name of the case for which the query is being made.
        /// </summary>
        public string CaseName { get; set; } = "";
        /// <summary>
        /// Gets or sets the hash of the query to be executed.
        /// </summary>
        public string QueryIdentifier { get; set; } = "";
        /// <summary>
        /// Gets or sets the query string that defines the graph query to be executed.
        /// </summary>
        public string Query { get; set; }
        public string QueryFormat { get; set; }

    }
}
