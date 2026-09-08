using FEAR.Domain.Provenance;

namespace FEAR.Domain.Infrastructure
{
    /// <summary>
    /// Provides a base implementation for evidence context objects, associating a provenance object with an evidence source/handler.
    /// An evidence context encapsulates details of a loaded evidence item, such as a disk, directory, file, network, memory, partition, volume, etc.
    /// This class is intended to be extended for specific evidence types and handlers.
    /// </summary>
    /// <typeparam name="T">The provenance type, implementing <see cref="IProvenance"/>.</typeparam>
    /// <typeparam name="R">The evidence source or handler type.</typeparam>
    public abstract class BaseEvidenceContext<T, R> : IEvidenceContext<T>
        where T : IProvenance
        where R : new()
    {
        /// <summary>
        /// Gets the provenance object associated with this context.
        /// </summary>
        public IProvenance Provenance => CaptureProvenance;

        /// <summary>
        /// Gets or sets the evidence source or handler associated with this context.
        /// </summary>
        protected R EvidenceSource { get; set; }

        /// <summary>
        /// Gets or sets the strongly-typed provenance object for this context.
        /// </summary>
        public T CaptureProvenance { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="BaseEvidenceContext{T, R}"/> class.
        /// </summary>
        /// <param name="evidenceProv">The provenance object to associate with this context.</param>
        /// <param name="handler">The evidence source or handler to associate with this context.</param>
        public BaseEvidenceContext(T evidenceProv, R handler)
        {
            CaptureProvenance = evidenceProv;
            EvidenceSource = handler;
        }

        /// <summary>
        /// Gets the evidence source or handler associated with this context.
        /// </summary>
        /// <returns>The evidence source or handler.</returns>
        public R GetEvidenceSource()
        {
            return EvidenceSource;
        }

        /// <summary>
        /// Gets the evidence source or handler as an object (for interface compatibility).
        /// </summary>
        /// <returns>The evidence source or handler as an object.</returns>
        object IEvidenceContext<T>.GetEvidenceSource()
        {
            return EvidenceSource;
        }
    }
}
