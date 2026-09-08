using FEAR.Domain.Infrastructure;
using FEAR.Domain.Provenance;

namespace FEAR.Domain.Model.Provenance
{
    /// <summary>
    /// Abstract base class for provenance objects that represent the capture of evidence from a source,
    /// this could be a disk image, network capture, or memory dump.
    /// Inherits from <see cref="BaseProvenance"/> and adds properties for associating an <see cref="EvidenceSource"/>.
    /// 
    /// <para>
    /// <b>Purpose:</b> This class is used to model provenance for evidence items that are directly
    /// captured from a source. It provides a contract for specifying whether local access is required
    /// (<see cref="LocalRequired"/>) and for linking the provenance to the actual evidence source (<see cref="EvidenceSource"/>).
    /// </para>
    /// 
    /// <para>
    /// <b>Usage:</b> Derived classes should implement the <see cref="LocalRequired"/> property to indicate if the evidence
    /// must be available locally (e.g., for imaging or direct access). The <see cref="EvidenceSource"/> property is used
    /// to encapsulate the retrieval and management of the underlying evidence data.
    /// </para>
    /// </summary>
    public abstract class CaptureProvenance : BaseProvenance
    {
        /// <summary>
        /// Gets a value indicating whether the evidence must be available locally for this provenance type.
        /// Derived classes must implement this property.
        /// </summary>
        public abstract bool LocalRequired { get; }

        /// <summary>
        /// Gets or sets the evidence source associated with this provenance.
        /// This links the provenance record to the actual data source (e.g., file, disk, stream).
        /// </summary>
        public EvidenceSource EvidenceSource { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="CaptureProvenance"/> class.
        /// </summary>
        public CaptureProvenance() : base() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="CaptureProvenance"/> class with a parent provenance and a name.
        /// </summary>
        /// <param name="parent">The parent provenance object in the hierarchy.</param>
        /// <param name="name">The name of this provenance object.</param>
        public CaptureProvenance(IProvenance parent, string name) : base(parent, name)
        {
        }
    }
}
