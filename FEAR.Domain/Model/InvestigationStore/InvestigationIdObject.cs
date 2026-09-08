namespace FEAR.Domain.Model.InvestigationStore
{
    /// <summary>
    /// Abstract base class providing a unique investigation identifier property.
    /// Intended for use by entities related to a specific investigation.
    /// </summary>
    public abstract class InvestigationIdObject
    {
        /// <summary>
        /// Gets or sets the unique identifier for the investigation associated with this entity.
        /// </summary>
        public virtual Guid InvestigationId { get; set; }
    }
}
