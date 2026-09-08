using FEAR.Domain.Infrastructure;
using AutoMapper;
using Newtonsoft.Json;
using FEAR.Domain.Evidence;
using FEAR.Domain.Model.Provenance;

namespace FEAR.Host.Core.Database
{
    /// <summary>
    /// Implements an evidence store backed by a database.
    /// Manages the addition and retrieval of evidence sources, including serialization and caching.
    /// </summary>
    public class DatabaseEvidenceStore : IEvidenceStore
    {
        private readonly IEvidenceSourceProvider _evidenceSourceProvider;
        private readonly InvestigationStoreDbContext _context;
        private readonly IMapper _mapper;

        /// <summary>
        /// In-memory cache for ephemeral evidence sources, keyed by provenance URI.
        /// </summary>
        protected Dictionary<string, EvidenceSourceEntry> EphemeralSources = new Dictionary<string, EvidenceSourceEntry>();

        /// <summary>
        /// Initializes a new instance of the <see cref="DatabaseEvidenceStore"/> class.
        /// </summary>
        /// <param name="context">The database context for evidence sources.</param>
        /// <param name="evidenceSourceProvider">The provider for evidence source operations.</param>
        /// <param name="mapper">The AutoMapper instance for object mapping.</param>
        public DatabaseEvidenceStore(InvestigationStoreDbContext context, IEvidenceSourceProvider evidenceSourceProvider, IMapper mapper)
        {
            _evidenceSourceProvider = evidenceSourceProvider;
            _context = context;
            _mapper = mapper;
        }

        /// <summary>
        /// Adds a new evidence source to the store or retrieves it if already present.
        /// Handles serialization, deserialization, and caching of evidence sources.
        /// </summary>
        /// <param name="sourceOpts">The options for the evidence source, including provenance capture.</param>
        /// <returns>The created or retrieved <see cref="EvidenceSource"/> instance.</returns>
        /// <exception cref="Exception">Thrown if the capture provenance does not have a ProvenanceId.</exception>
        public EvidenceSource AddEvidenceSource(EvidenceSourceOptions sourceOpts)
        {
            // Check if the evidence source is already cached in memory
            if (!EphemeralSources.ContainsKey(sourceOpts.Capture.GetProvenanceUri()))
            {
                // Ensure the capture provenance has a ProvenanceId
                if (!sourceOpts.Capture.ProvenanceId.HasValue)
                    throw new Exception("Capture Provenance must have a ProvenanceId. Add Capture to the Provenance Store before attaching it as an evidence source.");

                // Find the nearest parent investigation provenance ID
                var invId = sourceOpts.Capture.FindNearestParent<InvestigationProvenance>()?.ProvenanceId;

                EvidenceSource source;
                // Try to find an existing evidence source entry in the database
                var dbSourceOpts = _context.EvidenceSources.SingleOrDefault(t => t.CaptureProvenanceId == sourceOpts.Capture.ProvenanceId.Value);

                if (dbSourceOpts == null)
                {
                    // Create a new evidence source entry if not found
                    dbSourceOpts = new EvidenceSourceEntry();
                    dbSourceOpts.InvestigationId = invId.Value;
                    dbSourceOpts.CaptureProvenanceId = sourceOpts.Capture.ProvenanceId.Value;
                    dbSourceOpts.Capture = sourceOpts.Capture;
                    _context.EvidenceSources.Add(dbSourceOpts);
                }

                // Validate if the evidence source exists in the provider or needs to be created
                bool evidenceIsInStore = _evidenceSourceProvider.ValidateEvidenceSourceExists(sourceOpts);
                if (!evidenceIsInStore || String.IsNullOrEmpty(dbSourceOpts.SerializedEvidenceSource))
                {
                    // Save and serialize the evidence source if not present
                    source = _evidenceSourceProvider.SaveEvidenceSource(sourceOpts);

                    dbSourceOpts.EvidenceSourceSerializationType = source.GetType().AssemblyQualifiedName;
                    Type ty = source.GetType();
                    string tx = JsonConvert.SerializeObject(source, new JsonSerializerSettings() { TypeNameHandling = TypeNameHandling.All });
                    dbSourceOpts.SerializedEvidenceSource = tx;
                }
                else
                {
                    // Deserialize and open the evidence source if already stored
                    var sourceFromStore = (EvidenceSource)JsonConvert.DeserializeObject(dbSourceOpts.SerializedEvidenceSource, new JsonSerializerSettings() { TypeNameHandling = TypeNameHandling.Auto });
                    source = _evidenceSourceProvider.OpenEvidenceSource(sourceFromStore);
                }

                // Update the entry with the latest capture and evidence source
                dbSourceOpts.Capture = sourceOpts.Capture;
                dbSourceOpts.EvidenceSource = source;

                // Persist changes to the database
                _context.SaveChanges();
                // Cache the entry in memory
                EphemeralSources.Add(sourceOpts.Capture.GetProvenanceUri(), dbSourceOpts);
            }

            // Retrieve the evidence source from the cache and configure it
            var s = EphemeralSources[sourceOpts.Capture.GetProvenanceUri()];
            _evidenceSourceProvider.Configure(s.EvidenceSource);

            return s.EvidenceSource;
        }
    }
}
