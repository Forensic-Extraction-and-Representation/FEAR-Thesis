using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FEAR.Domain.Provenance
{
    /// <summary>
    /// Represents a persisted provenance record in the database.
    /// Implements <see cref="IProvenance"/> to provide storage and metadata for provenance objects,
    /// including serialization, hierarchy, and identification details.
    /// </summary>
    public class ProvenanceEntry : IProvenance
    {
        /// <summary>
        /// Gets or sets the unique identifier for this provenance entry (primary key).
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid ProvenanceId { get; set; }

        /// <summary>
        /// Gets or sets the unique identifier of the parent provenance entry, if any.
        /// </summary>
        public Guid? ParentProvenanceId { get; set; }

        /// <summary>
        /// Gets or sets the serialized representation of the provenance object (e.g., JSON).
        /// </summary>
        public string SerializedProvenance { get; set; }

        /// <summary>
        /// Gets or sets the display name of the provenance entry.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the path representing the location or hierarchy of this provenance entry.
        /// </summary>
        public string Path { get; set; }

        /// <summary>
        /// Gets or sets the URI element that uniquely identifies this provenance entry.
        /// </summary>
        public string ProvenanceUriElement { get; set; }

        /// <summary>
        /// Gets or sets the .NET type for this provenance entry.
        /// </summary>
        public string ProvenanceSerializationType { get; set; }

        /// <summary>
        /// Gets or sets the type of this provenance entry as a string.
        /// </summary>
        public string ProvenanceType { get; set; }

        /// <summary>
        /// Not implemented. Gets the parent provenance object in the hierarchy.
        /// </summary>
        [NotMapped]
        public IProvenance Parent => throw new NotImplementedException();

        /// <summary>
        /// Not implemented. Gets the child provenance objects in the hierarchy.
        /// </summary>
        [NotMapped]
        public IReadOnlyList<IProvenance> ChildItems => throw new NotImplementedException();

        /// <summary>
        /// Gets or sets the unique identifier for this provenance object (explicit interface implementation).
        /// </summary>
        Guid? IProvenance.ProvenanceId { get => ProvenanceId; set => ProvenanceId = value ?? ProvenanceId; }

        /// <summary>
        /// Not implemented. Adds a child provenance object to this provenance entry.
        /// </summary>
        /// <param name="evidence">The child provenance object to add.</param>
        public void AddChildProvenance(IProvenance evidence) => throw new NotImplementedException();

        /// <summary>
        /// Not implemented. Creates a file for the specified name and updates the provenance history.
        /// </summary>
        /// <param name="name">The name of the file to create.</param>
        /// <param name="history">The provenance history to update.</param>
        /// <returns>The path to the created file.</returns>
        public string CreateFileFor(string name, ref IList<IProvenance> history) => throw new NotImplementedException();

        /// <summary>
        /// Not implemented. Finds the nearest parent provenance object of the specified type in the hierarchy.
        /// </summary>
        /// <typeparam name="T">The type of parent provenance to find.</typeparam>
        /// <returns>The nearest parent of the specified type, or null if not found.</returns>
        public T FindNearestParent<T>() where T : IProvenance => throw new NotImplementedException();

        /// <summary>
        /// Not implemented. Gets the full URI for this provenance entry.
        /// </summary>
        /// <returns>The provenance URI as a string.</returns>
        public string GetProvenanceUri() => throw new NotImplementedException();

        /// <summary>
        /// Not implemented. Sets the provenance store that manages this provenance entry.
        /// </summary>
        /// <param name="store">The provenance store instance.</param>
        public void SetStore(IProvenanceStore store) => throw new NotImplementedException();
    }
}
