using FEAR.Domain.KnowledgeGraph.EntitySearch;
using HashStr = System.String;

namespace FEAR.Domain.KnowledgeGraph.GraphUpdate
{
    /// <summary>
    /// Represents the root entity for a graph update operation.
    /// The root entity is derived from the result object returned by GFEAR scripts.
    /// It aggregates all entities, collections, and identification paths relevant to the update.
    /// </summary>
    public class GraphUpdateRootEntity : GraphUpdateEntity
    {
        /// <summary>
        /// Gets a dictionary of entities that are identified solely by their literals or URIs.
        /// Each entity is uniquely identified by a hash generated from its property/values.
        /// An entity with a given hash should only occur once in this collection.
        /// </summary>
        public IDictionary<HashStr, GraphUpdateEntity> LeafEntities => _leafEntities.Value;
        private Lazy<IDictionary<HashStr, GraphUpdateEntity>> _leafEntities = null;

        /// <summary>
        /// Gets a list of collections associated with this root entity.
        /// Collections are related to an entity/property and are required to load information from the graph.
        /// After the initial load, this list is reduced to only include collections where the entity/property exists.
        /// </summary>
        public IList<GraphUpdateCollection> Collections => _collections.Value;
        private Lazy<IList<GraphUpdateCollection>> _collections = null;

        /// <summary>
        /// Gets a dictionary of entity identification paths.
        /// After loading literal/URI-identified entities, this list is reduced to only include
        /// paths where all entities are loaded. Paths may originate from the root entity or from
        /// loose entities (such as those in a collection).
        /// </summary>
        public IDictionary<HashStr, IdentifiedEntity> IdentifiedEntities => _identifyingEntities.Value;
        private Lazy<IDictionary<HashStr, IdentifiedEntity>> _identifyingEntities = null;

        /// <summary>
        /// Initializes a new instance of the <see cref="GraphUpdateRootEntity"/> class.
        /// Sets up lazy initialization for leaf entities, collections, and identified entities.
        /// </summary>
        /// <param name="ent">The root <see cref="Entity"/> for the update operation.</param>
        public GraphUpdateRootEntity(Entity ent) : base(ent, "E", new GraphUpdateEntityParentReference(), IdentifiedByCondition.None)
        {
            _leafEntities = new Lazy<IDictionary<HashStr, GraphUpdateEntity>>(() => ReturnLeafEntities());
            _collections = new Lazy<IList<GraphUpdateCollection>>(() => ReturnCollections());

            _identifyingEntities = new Lazy<IDictionary<HashStr, IdentifiedEntity>>(() =>
            {
                IDictionary<HashStr, IdentifiedEntity> idEntities = new Dictionary<HashStr, IdentifiedEntity>();
                ReturnIdentifyiedEntities(0, ref idEntities);
                return idEntities;
            });
        }
    }
}