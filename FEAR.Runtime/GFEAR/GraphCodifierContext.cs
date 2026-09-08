using FEAR.Domain.GraphCodifier;
using FEAR.Domain.KnowledgeGraph;
using FEAR.Domain.KnowledgeGraph.GraphCodify;
using Newtonsoft.Json;

namespace FEAR.GFEAR
{
    /// <summary>
    /// Provides the runtime context for a graph codifier execution.
    /// Used by each codifier to receive the data or artifact to be codified, interact with the graph service,
    /// and notify the system of return entities. Also allows the codifier to signal when codification is complete
    /// so entities can be updated or persisted in the knowledge graph.
    /// </summary>
    public class GraphCodifierContext : IGraphCodifierContext
    {
        /// <summary>
        /// Gets or sets the data or artifact to be codified, provided to the codifier for processing.
        /// </summary>
        public CodifierExpandoObject Data { get; set; }

        /// <summary>
        /// Gets the collection of entities that are the result of the codification process.
        /// </summary>
        public Dictionary<Uri, Entity> ResultEntities { get; protected set; } = new Dictionary<Uri, Entity>();

        private IFEARGraphCodifier GraphCodifier { get; set; }

        /// <summary>
        /// Gets the graph service for interacting with the knowledge graph (e.g., creating, updating, or querying entities).
        /// </summary>
        public IGraphCodifyService GraphService { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="GraphCodifierContext"/> class.
        /// </summary>
        /// <param name="graphCodifer">The codifier to execute.</param>
        /// <param name="graphService">The graph service for entity and relationship management.</param>
        public GraphCodifierContext(IFEARGraphCodifier graphCodifer, IGraphCodifyService graphService)
        {
            GraphCodifier = graphCodifer;
            GraphService = graphService;
        }

        /// <summary>
        /// Executes the codifier with the provided data, returning a deep copy of the codified data.
        /// </summary>
        /// <param name="data">The data or artifact to be codified.</param>
        /// <returns>A deep copy of the codified data.</returns>
        public object Execute(dynamic data)
        {
            Data = data;
            GraphCodifier.Execute(this);
            
            return JsonConvert.DeserializeObject(JsonConvert.SerializeObject(Data));
        }

        /// <summary>
        /// Notifies the context of an entity to be returned as a result of codification.
        /// </summary>
        /// <param name="entity">The entity to return.</param>
        public void AddReturnEntity(Entity entity)
        {
            if (!ResultEntities.ContainsKey(entity.Iri))
            {
                ResultEntities.Add(entity.Iri, entity);
            }
        }

        /// <summary>
        /// Finds or updates an entity in the graph, notifies the context of the resulting entity, and performs cleanup.
        /// </summary>
        /// <param name="ephemeralEntity">The entity to find or update.</param>
        public void FindOrUpdateEntity(Entity ephemeralEntity)
        {
            Entity material__entity = GraphService.CreateOrUpdateEntity(ephemeralEntity);

            AddReturnEntity(material__entity);

            GraphService.CleanupEphemeral();
        }
    }
}