using System.Text;
using System.Text.Json.Serialization;

namespace FEAR.Domain.Provenance
{
    /// <summary>
    /// Abstract base class for provenance objects, providing core logic for provenance hierarchy, storage, and URI construction.
    /// Implements <see cref="IProvenance"/> and supports parent/child relationships, provenance store integration, and recursive URI generation.
    /// </summary>
    public abstract class BaseProvenance : IProvenance
    {
        private IProvenanceStore _store = null;
        private IProvenance _parent = null;
        private readonly Lazy<IList<IProvenance>> _childItems;
        private string _provenanceUri = null;

        /// <summary>
        /// Gets or sets the unique identifier of the parent provenance object, if any.
        /// </summary>
        public Guid? ParentProvenanceId { get; set; } = null;

        /// <summary>
        /// Gets or sets the unique identifier for this provenance object.
        /// </summary>
        public Guid? ProvenanceId { get; set; }

        /// <summary>
        /// Gets the serialization type for this provenance object (e.g., the full type name).
        /// </summary>
        public string ProvenanceSerializationType => GetType().FullName;

        /// <summary>
        /// Gets or sets the name of this provenance object.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the path representing the location or hierarchy of this provenance object.
        /// </summary>
        public string Path { get; set; }

        /// <summary>
        /// Gets the URI element that uniquely identifies this provenance object.
        /// Derived classes must implement this property.
        /// </summary>
        public abstract string ProvenanceUriElement { get; }

        /// <summary>
        /// Gets the parent provenance object in the hierarchy, if any.
        /// </summary>
        [JsonIgnore]
        public IProvenance Parent
        {
            get => GetParent();
            protected set => SetParent(value);
        }

        /// <summary>
        /// Gets the child provenance objects in the hierarchy.
        /// </summary>
        [JsonIgnore]
        public IReadOnlyList<IProvenance> ChildItems => _childItems.Value.AsReadOnly();

        /// <summary>
        /// Gets the type of this provenance object as a string.
        /// </summary>
        public string ProvenanceType => GetType().Name;

        /// <summary>
        /// Initializes a new instance of the <see cref="BaseProvenance"/> class.
        /// </summary>
        public BaseProvenance()
        {
            _childItems = new Lazy<IList<IProvenance>>(() =>
            {
                if (ProvenanceId.HasValue && _store != null)
                {
                    var childItems = _store.GetProvenanceRecords(t => t.ParentProvenanceId == ProvenanceId);
                    return childItems;
                }
                else
                    return new List<IProvenance>();
            });
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BaseProvenance"/> class with a parent and name.
        /// </summary>
        /// <param name="parent">The parent provenance object.</param>
        /// <param name="name">The name of this provenance object.</param>
        public BaseProvenance(IProvenance parent, string name) : this()
        {
            Parent = parent;
            Name = name;
        }

        /// <summary>
        /// Sets the provenance store that manages this provenance object.
        /// This is used to retrieve and store provenance records 
        /// of both parent and child provenance objects.
        /// </summary>
        /// <param name="store">The provenance store instance.</param>
        public void SetStore(IProvenanceStore store)
        {
            _store = store;
        }

        /// <summary>
        /// Attempts to retrieve the parent provenance object from the store if it is not already set.
        /// </summary>
        private IProvenance GetParent()
        {
            if (_parent == null && ParentProvenanceId.HasValue)
            {
                _parent = _store?.GetProvenanceRecord(ParentProvenanceId.Value);
            }

            return _parent;
        }

        /// <summary>
        /// Sets the parent provenance object and updates the store if necessary.
        /// </summary>
        private void SetParent(IProvenance value)
        {
            if (value?.ProvenanceId != ParentProvenanceId)
            {
                ParentProvenanceId = value.ProvenanceId;
                _store?.AddOrUpdateProvenanceRecord(this);
            }

            _parent = value;
        }

        /// <summary>
        /// Adds a child provenance object to this provenance object.
        /// Ensures no duplicate child with the same name and path is added.
        /// </summary>
        /// <param name="evidence">The child provenance object to add.</param>
        public void AddChildProvenance(IProvenance evidence)
        {
            if (_childItems.Value.Any(t => t.Name == evidence.Name && t.Path == evidence.Path))
                return;

            // Visit the evidence item with this being the parent
            ((BaseProvenance)evidence).ParentVisit(this);

            // Ensure the evidence has a valid ProvenanceId
            if (!evidence.ProvenanceId.HasValue && _store != null)
                _store.AddOrUpdateProvenanceRecord(evidence);

            _childItems.Value.Add(evidence);
        }

        /// <summary>
        /// Visits the parent provenance object and updates the ParentProvenanceId.
        /// </summary>
        /// <remarks>
        /// This is very similar to the SetParent method, but it is used when
        /// visiting a parent. The main difference is the parent is assumed
        /// to have created the ProvenanceRecord already
        /// </remarks>
        /// <param name="parent"></param>
        private void ParentVisit(IProvenance parent)
        {
            ParentProvenanceId = parent.ProvenanceId;
            Parent = parent;
        }

        /// <summary>
        /// Creates a file for the specified name and updates the provenance history.
        /// Recursively calls the parent until the root is reached.
        /// </summary>
        /// <param name="name">The name of the file to create.</param>
        /// <param name="history">The provenance history to update.</param>
        /// <returns>The path to the created file.</returns>
        public virtual string CreateFileFor(string name, ref IList<IProvenance> history)
        {
            history.Add(this);
            if (Parent != null)
                return Parent.CreateFileFor(name, ref history);
            else
                throw new Exception("No provenance back to investigation.");
        }

        /// <summary>
        /// Gets the full URI for this provenance object by recursively combining parent URIs and the current URI element.
        /// </summary>
        /// <returns>The provenance URI as a string.</returns>
        public string GetProvenanceUri()
        {
            if (_provenanceUri == null)
            {
                StringBuilder sb = new StringBuilder();
                // Construct the URI out of the current urielement and the parent's urielement
                // If there is no parent, then we are at the root of the provenance tree
                // and we can just return the current urielement
                sb.Append(Parent?.GetProvenanceUri() ?? "");
                sb.Append($"{ProvenanceUriElement.TrimEnd('/')}/");
                _provenanceUri = sb.ToString();
            }

            return _provenanceUri;
        }

        /// <summary>
        /// Finds the nearest parent provenance object of the specified type in the hierarchy.
        /// </summary>
        /// <typeparam name="T">The type of parent provenance to find.</typeparam>
        /// <returns>The nearest parent of the specified type, or throws if not found.</returns>
        public T FindNearestParent<T>()
            where T : IProvenance
        {
            IProvenance current = this;
            while (current != null)
            {
                if (current is T)
                    return (T)current;

                current = current.Parent;
            }

            throw new Exception($"No parent of type {typeof(T).Name} found.");
        } 
    }
}
