using FEAR.Domain.Infrastructure;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using FEAR.Domain.Evidence;
using FEAR.Domain.Model.Provenance;

namespace FEAR.Host.Core.Database
{
    /// <summary>
    /// Represents a database entity for storing evidence source information.
    /// Inherits from <see cref="EvidenceSourceOptions"/> and adds persistence-related fields.
    /// Used to persist and retrieve evidence source metadata, serialization, and provenance linkage in the database.
    /// </summary>
    public class EvidenceSourceEntry : EvidenceSourceOptions
    {
        /// <summary>
        /// Gets or sets the unique identifier for this evidence source entry (primary key).
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid EvidenceSourceId { get; set; }

        /// <summary>
        /// Gets or sets the investigation ID associated with this evidence source.
        /// </summary>
        public Guid InvestigationId { get; set; }

        /// <summary>
        /// Gets or sets the provenance ID of the capture associated with this evidence source.
        /// </summary>
        public Guid CaptureProvenanceId { get; set; }

        /// <summary>
        /// Gets or sets the .NET type name used for serializing the evidence source.
        /// </summary>
        public string EvidenceSourceSerializationType { get; set; }

        /// <summary>
        /// Gets or sets the serialized evidence source object (as JSON or other format).
        /// </summary>
        public string SerializedEvidenceSource { get; set; }

        /// <summary>
        /// Gets or sets the provenance capture object associated with this evidence source.
        /// Not mapped to the database.
        /// </summary>
        [NotMapped]
        public override CaptureProvenance Capture { get; set; }

        /// <summary>
        /// Gets or sets the in-memory evidence source object.
        /// Not mapped to the database.
        /// </summary>
        [NotMapped]
        public EvidenceSource EvidenceSource { get; set; }
    }
}
