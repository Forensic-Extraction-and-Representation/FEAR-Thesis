using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FEAR.Domain.Dto.InvestigationStore
{
    /// <summary>
    /// Represents a configuration for an investigation, including its unique identifier, configuration data, and hosted URI.
    /// </summary>
    [Table("InvestigationConfigurations")]
    public class InvestigationConfiguration : Model.InvestigationStore.InvestigationConfiguration
    {
        /// <summary>
        /// Gets or sets the unique identifier for this investigation configuration.
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public override Guid InvestigationConfigurationId { get; set; }

        /// <summary>
        /// Gets or sets the URI where the investigation is hosted or accessible.
        /// This may point to a remote service, resource, or endpoint associated with the investigation.
        /// </summary>
        public string HostedUri { get; set; }
    }
}
