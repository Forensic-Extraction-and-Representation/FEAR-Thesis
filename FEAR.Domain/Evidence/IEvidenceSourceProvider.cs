using FEAR.Domain.Infrastructure;

namespace FEAR.Domain.Evidence
{
    /// <summary>
    /// Defines methods for managing and interacting with evidence sources.
    /// </summary>
    public interface IEvidenceSourceProvider
    {
        /// <summary>
        /// Opens an existing evidence source for access or processing.
        /// </summary>
        /// <param name="sourceFromStore">The evidence source instance retrieved from the store.</param>
        /// <returns>An <see cref="EvidenceSource"/> object ready for use.</returns>
        EvidenceSource OpenEvidenceSource(EvidenceSource sourceFromStore);

        /// <summary>
        /// Creates and saves a new evidence source using the specified options.
        /// </summary>
        /// <param name="options">The configuration options for the evidence source.</param>
        /// <returns>The newly created <see cref="EvidenceSource"/>.</returns>
        EvidenceSource SaveEvidenceSource(EvidenceSourceOptions options);

        /// <summary>
        /// Validates whether an evidence source exists based on the provided options.
        /// </summary>
        /// <param name="sourceOpts">The options describing the evidence source to validate.</param>
        /// <returns><c>true</c> if the evidence source exists; otherwise, <c>false</c>.</returns>
        bool ValidateEvidenceSourceExists(EvidenceSourceOptions sourceOpts);

        /// <summary>
        /// Configures an evidence source with the necessary settings and dependencies from the provider.
        /// </summary>
        /// <param name="s">The evidence source to visit.</param>
        void Configure(EvidenceSource s);
    }
}
