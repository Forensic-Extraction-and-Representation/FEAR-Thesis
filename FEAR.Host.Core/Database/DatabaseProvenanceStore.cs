using FEAR.Domain.Provenance;
using AutoMapper;

namespace FEAR.Host.Core.Database
{
    /// <summary>
    /// Implements a provenance store backed by a database.
    /// Handles storage, retrieval, and querying of provenance records, which track the origin and history of evidence or entities.
    /// </summary>
    public class DatabaseProvenanceStore : IProvenanceStore
    {
        private readonly InvestigationStoreDbContext _context;
        private readonly IMapper _mapper;

        /// <summary>
        /// Initializes a new instance of the <see cref="DatabaseProvenanceStore"/> class.
        /// </summary>
        /// <param name="context">The database context for provenance entries.</param>
        /// <param name="mapper">The AutoMapper instance for object mapping.</param>
        public DatabaseProvenanceStore(InvestigationStoreDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        /// <summary>
        /// Adds a new provenance record or updates an existing one in the database.
        /// Serializes the provenance object for storage and updates the ProvenanceId if necessary.
        /// </summary>
        /// <param name="provenance">The provenance object to add or update.</param>
        public void AddOrUpdateProvenanceRecord(IProvenance provenance)
        {
            if (_context.ProvenanceEntries.SingleOrDefault(x => x.ProvenanceId == provenance.ProvenanceId) != null)
            {
                _context.ProvenanceEntries.Update(new ProvenanceEntry()
                {
                    ProvenanceId = provenance.ProvenanceId.Value,
                    ParentProvenanceId = provenance.ParentProvenanceId,
                    ProvenanceType = provenance.ProvenanceType,
                    Name = provenance.Name,
                    Path = provenance.Path,
                    ProvenanceUriElement = provenance.ProvenanceUriElement,
                    SerializedProvenance = System.Text.Json.JsonSerializer.Serialize(provenance, provenance.GetType())
                });
            }
            else
            {
                provenance.SetStore(this);
                var entity = new ProvenanceEntry()
                {
                    ParentProvenanceId = provenance.ParentProvenanceId,
                    ProvenanceType = provenance.ProvenanceType,
                    Name = provenance.Name,
                    Path = provenance.Path,
                    ProvenanceUriElement = provenance.ProvenanceUriElement,
                    ProvenanceSerializationType = provenance.GetType().AssemblyQualifiedName,
                };

                _context.ProvenanceEntries.Add(entity);
                provenance.ProvenanceId = entity.ProvenanceId;
                entity.SerializedProvenance = System.Text.Json.JsonSerializer.Serialize(provenance, provenance.GetType());
            }

            _context.SaveChanges();
        }

        /// <summary>
        /// Retrieves a provenance record by its unique identifier.
        /// Deserializes the stored provenance object and sets its store reference.
        /// </summary>
        /// <param name="recordId">The unique identifier of the provenance record.</param>
        /// <returns>The deserialized provenance object, or null if not found.</returns>
        public IProvenance GetProvenanceRecord(Guid recordId)
        {
            var record = _context.ProvenanceEntries.FirstOrDefault(x => x.ProvenanceId == recordId);
            if (record != null)
            {
                var prov = (IProvenance)System.Text.Json.JsonSerializer.Deserialize(record.SerializedProvenance, Type.GetType(record.ProvenanceSerializationType));
                prov.SetStore(this);
                return prov;
            }

            return null;
        }

        /// <summary>
        /// Retrieves a list of provenance records that match the given predicate.
        /// </summary>
        /// <param name="predicate">A function to filter provenance records.</param>
        /// <returns>A list of provenance objects that match the predicate.</returns>
        public IList<IProvenance> GetProvenanceRecords(Func<IProvenance, bool> predicate)
        {
            var pe = _context.ProvenanceEntries.Where(predicate).ToList();

            return pe.Select(x => (IProvenance)ProvenanceDeserialize(x as ProvenanceEntry)).ToList();
        }

        /// <summary>
        /// Retrieves a list of provenance records of a specific type that match the given predicate.
        /// </summary>
        /// <typeparam name="T">The type of provenance to retrieve.</typeparam>
        /// <param name="predicate">A function to filter provenance records.</param>
        /// <returns>A list of provenance records of type <typeparamref name="T"/> that match the predicate.</returns>
        public IList<T> GetProvenanceRecords<T>(Func<IProvenance, bool> predicate)
            where T : IProvenance
        {
            var pe = _context.ProvenanceEntries.Where(x => x.ProvenanceSerializationType == typeof(T).AssemblyQualifiedName).Where(predicate).ToList();

            return pe.Select(x => (T)ProvenanceDeserialize(x as ProvenanceEntry)).ToList();
        }

        /// <summary>
        /// Deserializes a <see cref="ProvenanceEntry"/> into an <see cref="IProvenance"/> object and sets its store reference.
        /// </summary>
        /// <param name="x">The provenance entry to deserialize.</param>
        /// <returns>The deserialized provenance object.</returns>
        internal object ProvenanceDeserialize(ProvenanceEntry x)
        {
            var obj = (IProvenance)System.Text.Json.JsonSerializer.Deserialize(x.SerializedProvenance, Type.GetType(x.ProvenanceSerializationType));
            obj.SetStore(this);
            return obj;
        }
    }
}
