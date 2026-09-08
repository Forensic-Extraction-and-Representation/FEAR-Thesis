using Microsoft.Extensions.Logging;
using System.Text.Json.Serialization;

namespace FEAR.Domain.Infrastructure
{
    /// <summary>
    /// Represents a set of evidence sources grouped together under a single logical unit.
    /// Inherits from <see cref="EvidenceSource"/> and allows management of multiple 
    /// named <see cref="EvidenceSource"/> instances.
    /// Useful for scenarios where evidence is composed of multiple files, streams, 
    /// or sources that need to be managed collectively (eg, EWF files)
    /// </summary>
    public abstract class EvidenceSourceSet : EvidenceSource
    {
        /// <summary>
        /// Gets a value indicating that this evidence source is a set (always true for sets).
        /// </summary>
        public bool IsSet { get; } = true;
        
        /// <summary>
        /// Gets the dictionary of named evidence sources in this set.
        /// The key is a string identifier for the source, and the value is the <see cref="EvidenceSource"/> instance.
        /// </summary>
        [JsonInclude]
        public IDictionary<string, EvidenceSource> SourceSet { get; protected set; } = new Dictionary<string, EvidenceSource>();

        /// <summary>
        /// Initializes a new instance of the <see cref="EvidenceSourceSet"/> class.
        /// </summary>
        public EvidenceSourceSet() : base() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="EvidenceSourceSet"/> class with the specified path, investigation ID, and logger.
        /// </summary>
        /// <param name="path">The path or location of the evidence source set.</param>
        /// <param name="investigationId">The investigation ID associated with this evidence source set.</param>
        /// <param name="logger">The logger to use for this evidence source set.</param>
        public EvidenceSourceSet(string path, Guid investigationId, ILogger logger) : base(path, investigationId, logger)
        {
        }

        /// <summary>
        /// Retrieves an evidence source from the set by its name.
        /// </summary>
        /// <param name="name">The name or key of the evidence source.</param>
        /// <returns>The <see cref="EvidenceSource"/> associated with the specified name.</returns>
        public EvidenceSource Get(string name)
        {
            return SourceSet[name];
        }

        /// <summary>
        /// Adds a new evidence source to the set with the specified name.
        /// </summary>
        /// <param name="name">The name or key for the evidence source.</param>
        /// <param name="fileSource">The <see cref="EvidenceSource"/> to add.</param>
        public void Add(string name, EvidenceSource fileSource)
        {
            SourceSet.Add(name, fileSource);
        }
    }
}
