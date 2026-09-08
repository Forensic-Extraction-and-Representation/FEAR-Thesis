using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FEAR.Domain.Dto
{
    /// <summary>
    /// Represents configuration options for a forensic tool, including executable details, arguments, and input/output requirements.
    /// Inherits common tool option properties from <see cref="FEAR.Domain.Models.ToolOptions"/>.
    /// </summary>
    [Table("ToolOptions")]
    public class ToolOptions : FEAR.Domain.Model.ToolOptions
    {
        /// <summary>
        /// Gets or sets the unique identifier for this tool option configuration.
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public override Guid ToolOptionId { get; set; }
    }
}
