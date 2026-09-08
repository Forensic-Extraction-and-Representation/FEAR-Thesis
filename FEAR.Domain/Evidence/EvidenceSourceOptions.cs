using FEAR.Domain.Model.Provenance;

namespace FEAR.Domain.Evidence
{
    /// <summary>
    /// Represents configuration options for an evidence source, including provenance capture and locality requirements.
    /// </summary>
    public class EvidenceSourceOptions
    {
        /// <summary>
        /// Gets or sets the provenance capture strategy for the evidence source.
        /// This defines how and what provenance information is collected for the evidence.
        /// </summary>
        public virtual CaptureProvenance Capture { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the evidence source must be available locally.
        /// If true, the evidence source must be local to the Hosted Service to be processed.
        /// </summary>
        public bool LocalRequired { get; set; }
    }
}
