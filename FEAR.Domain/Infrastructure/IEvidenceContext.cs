using FEAR.Domain.Provenance;

namespace FEAR.Domain.Infrastructure
{
    /// <summary>
    /// Defines a context for managing a loaded evidence in relation to its provenance.
    /// Provides access to the provenance object that defines where the evidence came from and how it was handled.
    /// </summary>
    public interface IEvidenceContext
    {
        /// <summary>
        /// Gets the provenance object associated with this evidence context.
        /// </summary>
        IProvenance Provenance { get; }
    }

    /// <summary>
    /// Generic interface for evidence contexts that provide strongly-typed access to provenance and the underlying evidence source or handler.
    /// </summary>
    /// <typeparam name="T">The provenance type, implementing <see cref="IProvenance"/>.</typeparam>
    public interface IEvidenceContext<T> : IEvidenceContext
        where T : IProvenance
    {
        /// <summary>
        /// Gets the strongly-typed provenance object associated with this evidence context.
        /// </summary>
        T CaptureProvenance { get; }

        /// <summary>
        /// Gets the evidence source or handler associated with this context as an object.
        /// </summary>
        /// <returns>The evidence source or handler.</returns>
        object GetEvidenceSource();
    }
}
