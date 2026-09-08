using FEAR.Domain.Infrastructure;

namespace FEAR.Domain.Evidence
{
    /// <summary>
    /// Defines a contract for storing and managing evidence sources.
    /// </summary>
    public interface IEvidenceStore
    {
        /// <summary>
        /// Adds a new evidence source to the store using the specified options.
        /// </summary>
        /// <param name="prov">The configuration options for the evidence source, including provenance capture and locality requirements.</param>
        /// <returns>The created <see cref="EvidenceSource"/> instance representing the stored evidence source.</returns>
        EvidenceSource AddEvidenceSource(EvidenceSourceOptions prov);
    }
}
