using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FEAR.Domain.Dto.InvestigationStore
{
    /// <summary>
    /// Represents a query that is stored centrally and can be shared between investigators within a case.
    /// Includes metadata for auditing and ownership, as well as the query definition and description.
    /// </summary>
    [Table("InvestigationQuerys")]
    public class InvestigationQuery : Model.InvestigationStore.InvestigationQuery
    {
        /// <summary>
        /// Gets or sets the unique identifier for this investigation query.
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public override Guid InvestigationQueryId { get; set; }
    }
}
