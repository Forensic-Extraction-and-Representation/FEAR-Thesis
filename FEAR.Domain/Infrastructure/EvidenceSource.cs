using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace FEAR.Domain.Infrastructure
{
    /// <summary>
    /// Abstract base class representing a source of evidence for an investigation.
    /// Provides lazy stream access, logging, and investigation context.
    /// Evidence Sources are used to encapsulate the retrieval of evidence data, 
    /// which can come from various sources such as files, databases, or remote services.
    /// Derived classes must implement <see cref="GetEvidenceStream"/> to provide the actual evidence data stream.
    /// </summary>
    public abstract class EvidenceSource
    {
        private Lazy<Stream> _stream;

        /// <summary>
        /// Gets or sets the unique identifier of the investigation associated with this evidence source.
        /// </summary>
        public Guid InvestigationId { get; set; }

        /// <summary>
        /// Gets the evidence data stream, initialized lazily via <see cref="GetEvidenceStream"/>.
        /// </summary>
        [JsonIgnore]
        public Stream Stream => _stream.Value;

        /// <summary>
        /// Gets or sets the logger for this evidence source. Not serialized.
        /// </summary>
        [JsonIgnore]
        protected ILogger Logger { get; set; }

        /// <summary>
        /// When overridden in a derived class, provides the stream for the evidence data.
        /// </summary>
        /// <param name="source">The evidence source instance.</param>
        /// <returns>The evidence data stream.</returns>
        public abstract Stream GetEvidenceStream(EvidenceSource source);

        /// <summary>
        /// Gets a value indicating whether the evidence source is set or initialized.
        /// </summary>
        public virtual bool IsSet { get; } = false;

        /// <summary>
        /// Gets the path or location of the evidence source.
        /// </summary>
        public string Path { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="EvidenceSource"/> class.
        /// </summary>
        public EvidenceSource() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="EvidenceSource"/> class with the specified path, investigation ID, and logger.
        /// </summary>
        /// <param name="path">The path or location of the evidence source.</param>
        /// <param name="investigationId">The investigation ID associated with this evidence source.</param>
        /// <param name="logger">The logger to use for this evidence source.</param>
        public EvidenceSource(string path, Guid investigationId, ILogger logger)
        {
            Path = path;
            InvestigationId = investigationId;
            _stream = new Lazy<Stream>(() =>
            {
                return GetEvidenceStream(this);
            });
        }

        /// <summary>
        /// Configures the logger for this evidence source.
        /// </summary>
        /// <param name="logger">The logger to use.</param>
                public void Configure(ILogger logger)
        {
            Logger = logger;
        }
    }
}
