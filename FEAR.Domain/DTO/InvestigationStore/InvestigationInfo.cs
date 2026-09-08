using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FEAR.Domain.Dto.InvestigationStore
{
    /// <summary>
    /// Represents summary information about an investigation, including creation, modification, and ownership details.
    /// </summary>
    [Table("Investigations")]
    public class InvestigationInfo : Model.InvestigationStore.InvestigationInfo
    {
        /// <summary>
        /// Gets or sets the unique identifier for this investigation.
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public override Guid InvestigationId { get; set; }

        /// <summary>
        /// Gets or sets the unique identifier of the user who created the investigation.
        /// </summary>
        public Guid CreatedByUserId { get; set; }

        /// <summary>
        /// Gets or sets the date and time when the investigation was created.
        /// </summary>
        public DateTime CreatedOn { get; set; }

        /// <summary>
        /// Gets or sets the unique identifier of the user who last modified the investigation.
        /// </summary>
        public Guid LastModifiedByUserId { get; set; }

        /// <summary>
        /// Gets or sets the date and time when the investigation was last modified.
        /// </summary>
        public DateTime LastModifiedOn { get; set; }
    }
}
