namespace FEAR.Domain.Model.InvestigationStore
{
    /// <summary>
    /// Represents a query that is stored centrally and can be shared between investigators within a case.
    /// Includes metadata for auditing, ownership, and descriptive information.
    /// </summary>
    public class InvestigationQuery
    {
        /// <summary>
        /// Gets or sets the unique identifier for this investigation query.
        /// </summary>
        public virtual Guid InvestigationQueryId { get; set; }

        /// <summary>
        /// Gets or sets the unique identifier of the investigation this query belongs to.
        /// </summary>
        public Guid InvestigationId { get; set; }

        /// <summary>
        /// Gets or sets the name of the query.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the actual query text (e.g., SPARQL, SQL, etc.).
        /// </summary>
        public string QueryText { get; set; }

        /// <summary>
        /// Gets or sets a description of the query's purpose or usage.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the unique identifier of the user who created the query.
        /// </summary>
        public Guid CreatedByUserId { get; set; }

        /// <summary>
        /// Gets or sets the date and time when the query was created.
        /// </summary>
        public DateTime CreatedOn { get; set; }

        /// <summary>
        /// Gets or sets the unique identifier of the user who last modified the query.
        /// </summary>
        public Guid LastModifiedByUserId { get; set; }

        /// <summary>
        /// Gets or sets the date and time when the query was last modified.
        /// </summary>
        public DateTime LastModifiedOn { get; set; }
    }
}
