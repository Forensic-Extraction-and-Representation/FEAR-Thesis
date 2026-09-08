namespace FEAR.Domain.KnowledgeGraph.IRI
{
    /// <summary>
    /// Provides a base implementation for generating IRIs (Internationalized Resource Identifiers) for entities in the knowledge graph.
    /// Uses a namespace abbreviation to ensure IRIs are unique and context-specific.
    /// </summary>
    public abstract class BaseIRIGenerator : IIRIGenerator
    {
        /// <summary>
        /// Gets the namespace abbreviation used as the prefix for generated IRIs.
        /// </summary>
        public string NamespaceAbbreviation { get; protected set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="BaseIRIGenerator"/> class with the specified namespace abbreviation.
        /// </summary>
        /// <param name="namespaceAbbrev">The namespace abbreviation to use for IRI generation.</param>
        public BaseIRIGenerator(string namespaceAbbrev)
        {
            NamespaceAbbreviation = namespaceAbbrev;
        }

        /// <summary>
        /// Returns the current Unix time in seconds since the epoch.
        /// </summary>
        private int UnixTime()
        {
            return (int)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
        }

        /// <summary>
        /// Creates a new IRI for an entity, using the namespace abbreviation and either a provided segment or a generated GUID.
        /// </summary>
        /// <param name="type">The type of entity (used in the IRI if no segment is provided).</param>
        /// <param name="uriSegment">A custom segment for the IRI. If null or empty, a GUID-based segment is used.</param>
        /// <returns>A new <see cref="Uri"/> representing the entity's IRI.</returns>
        public virtual Uri CreateIRI(string type, string uriSegment)
        {
            if (string.IsNullOrEmpty(uriSegment))
                return new Uri($"{NamespaceAbbreviation}:{type}-{Guid.NewGuid().ToString("N").ToLower().Substring(0, 10)}");
            else
                return new Uri($"{NamespaceAbbreviation}:{uriSegment.TrimStart('/')}");
        }

        /// <summary>
        /// Resets the generator state. Override in derived classes if stateful behavior is needed.
        /// </summary>
        public virtual void Reset()
        {
        }
    }
}