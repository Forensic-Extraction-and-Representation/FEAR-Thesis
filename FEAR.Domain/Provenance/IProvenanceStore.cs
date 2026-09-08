using FEAR.Domain.Provenance;
using System.Linq.Expressions;

namespace FEAR.Domain.Provenance
{
    /// <summary>
    /// Defines a contract for a provenance store, which manages the storage, retrieval, and querying of provenance records.
    /// Provenance records track the origin, history, and relationships of evidence or entities, and are centrally stored and shareable.
    /// </summary>
    public interface IProvenanceStore
    {
        /// <summary>
        /// Adds a new provenance record or updates an existing one in the store.
        /// </summary>
        /// <param name="provenance">The provenance object to add or update.</param>
        void AddOrUpdateProvenanceRecord(IProvenance provenance);

        /// <summary>
        /// Retrieves a provenance record by its unique identifier.
        /// </summary>
        /// <param name="recordId">The unique identifier of the provenance record.</param>
        /// <returns>The provenance object with the specified ID, or null if not found.</returns>
        IProvenance GetProvenanceRecord(Guid recordId);

        /// <summary>
        /// Retrieves a list of provenance records of a specific type that match the given predicate.
        /// </summary>
        /// <typeparam name="T">The type of provenance to retrieve.</typeparam>
        /// <param name="predicate">A function to filter provenance records.</param>
        /// <returns>A list of provenance records of type <typeparamref name="T"/> that match the predicate.</returns>
        IList<T> GetProvenanceRecords<T>(Func<IProvenance, bool> predicate) where T : IProvenance;

        /// <summary>
        /// Retrieves a list of provenance records that match the given predicate.
        /// </summary>
        /// <param name="predicate">A function to filter provenance records.</param>
        /// <returns>A list of provenance records that match the predicate.</returns>
        IList<IProvenance> GetProvenanceRecords(Func<IProvenance, bool> predicate);
    }
}