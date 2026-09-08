using System.Text.Json.Serialization;

namespace FEAR.Domain.Provenance
{
    /// <summary>
    /// Represents a provenance object, which tracks the origin, history, and relationships of evidence or entities.
    /// Provenance objects can be organized in a hierarchy, support serialization, and are centrally stored and shareable between investigators.
    /// </summary>
    public interface IProvenance
    {
        /// <summary>
        /// Sets the provenance store that manages this provenance object.
        /// </summary>
        /// <param name="store">The provenance store instance.</param>
        void SetStore(IProvenanceStore store);

        /// <summary>
        /// Gets the parent provenance object in the hierarchy, if any.
        /// </summary>
        [JsonIgnore]
        IProvenance Parent { get; }

        /// <summary>
        /// Gets the name of this provenance object.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Gets the path representing the location or hierarchy of this provenance object.
        /// </summary>
        string Path { get; }

        /// <summary>
        /// Gets the URI element that uniquely identifies this provenance object.
        /// </summary>
        string ProvenanceUriElement { get; }

        /// <summary>
        /// Gets the type of this provenance object as a string.
        /// </summary>
        string ProvenanceType { get; }

        /// <summary>
        /// Gets the child provenance objects in the hierarchy.
        /// </summary>
        [JsonIgnore]
        IReadOnlyList<IProvenance> ChildItems { get; }

        /// <summary>
        /// Gets the unique identifier of the parent provenance object, if any.
        /// </summary>
        Guid? ParentProvenanceId { get; }

        /// <summary>
        /// Gets or sets the unique identifier for this provenance object.
        /// </summary>
        Guid? ProvenanceId { get; set; }

        /// <summary>
        /// Gets the serialization type for this provenance object (e.g., JSON, XML).
        /// </summary>
        string ProvenanceSerializationType { get; }

        /// <summary>
        /// Adds a child provenance object to this provenance object.
        /// </summary>
        /// <param name="evidence">The child provenance object to add.</param>
        void AddChildProvenance(IProvenance evidence);

        /// <summary>
        /// Creates a file for the specified name and updates the provenance history.
        /// </summary>
        /// <param name="name">The name of the file to create.</param>
        /// <param name="history">The provenance history to update.</param>
        /// <returns>The path to the created file.</returns>
        string CreateFileFor(string name, ref IList<IProvenance> history);

        /// <summary>
        /// Gets the full URI for this provenance object.
        /// </summary>
        /// <returns>The provenance URI as a string.</returns>
        string GetProvenanceUri();

        /// <summary>
        /// Finds the nearest parent provenance object of the specified type in the hierarchy.
        /// </summary>
        /// <typeparam name="T">The type of parent provenance to find.</typeparam>
        /// <returns>The nearest parent of the specified type, or null if not found.</returns>
        T FindNearestParent<T>() where T : IProvenance;
    }
}
