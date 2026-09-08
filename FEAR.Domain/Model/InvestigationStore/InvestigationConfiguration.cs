namespace FEAR.Domain.Model.InvestigationStore
{
    /// <summary>
    /// Represents the configuration details for a specific investigation.
    /// Inherits the investigation identifier from <see cref="InvestigationIdObject"/>.
    /// </summary>
    public class InvestigationConfiguration : InvestigationIdObject
    {
        /// <summary>
        /// Gets or sets the unique identifier for this investigation configuration.
        /// </summary>
        public virtual Guid InvestigationConfigurationId { get; set; }

        /// <summary>
        /// Gets or sets the serialized configuration data for the investigation.
        /// This may include settings, parameters, or other configuration details in a structured format (e.g., JSON).
        /// </summary>
        public CaseConfiguration Configuration { get; set; }
    }
}
