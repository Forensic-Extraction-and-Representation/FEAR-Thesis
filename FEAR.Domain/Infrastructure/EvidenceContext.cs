using FEAR.Domain.Provenance;

namespace FEAR.Domain.Infrastructure
{
    /// <summary>
    /// Provides a concrete base for evidence context objects that associate a specific provenance type with an evidence source or handler.
    /// An evidence context encapsulates details of a loaded evidence item, such as a disk, directory, file, network, memory, partition, volume, etc.
    /// This class extends <see cref="BaseEvidenceContext{T, R}"/> and is intended for use with provenance types derived from <see cref="BaseProvenance"/>.
    /// It enables consistent management and access to provenance and evidence source information for evidence processing workflows.
    /// </summary>
    /// <typeparam name="T">The provenance type, derived from <see cref="BaseProvenance"/>.</typeparam>
    /// <typeparam name="R">The evidence source or handler type.</typeparam>
    public abstract class EvidenceContext<T, R> : BaseEvidenceContext<T, R>
        where T : BaseProvenance
        where R : new()
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EvidenceContext{T, R}"/> class,
        /// associating the specified provenance object and evidence source/handler.
        /// </summary>
        /// <param name="evidenceProv">The provenance object to associate with this context.</param>
        /// <param name="handler">The evidence source or handler to associate with this context.</param>
        public EvidenceContext(T evidenceProv, R handler) : base(evidenceProv, handler)
        {
        }
    }
}
