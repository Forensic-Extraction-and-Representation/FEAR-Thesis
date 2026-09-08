using FEAR.Domain.KnowledgeGraph.GraphCodify;
using FEAR.Domain.KnowledgeGraph.GraphManager;
using FEAR.Domain.KnowledgeGraph.Graphs;
using VDS.RDF;

namespace FEAR.Domain.KnowledgeGraph.GraphUpdate
{
    /// <summary>
    /// Provides context and state management for graph update operations.
    /// Tracks asserted and retracted triples, manages the root entity for updates,
    /// and handles temporary graph storage and change tracking.
    /// </summary>
    public class GraphUpdateContext
    {
        /// <summary>
        /// Indicates whether a remote load operation has been performed.
        /// </summary>
        public bool RemoteLoadPerformed = false;

        /// <summary>
        /// Gets or sets the list of triples that have been asserted (added) during the update.
        /// </summary>
        public IList<Triple> Asserted { get; set; } = new List<Triple>();

        /// <summary>
        /// Gets or sets the list of triples that have been retracted (removed) during the update.
        /// </summary>
        public IList<Triple> Retracted { get; set; } = new List<Triple>();

        /// <summary>
        /// Gets a string representation of the intermediate entity's view at depth 0.
        /// </summary>
        public string EntityView => IntermediateEntity.EntityView(0);

        /// <summary>
        /// Gets the intermediate entity being updated or managed in this context.
        /// </summary>
        public Entity IntermediateEntity { get; }

        /// <summary>
        /// Gets the graph manager responsible for graph operations.
        /// </summary>
        public IGraphManager GraphManager { get; }

        private GraphUpdateContext()
        {
            _rootUpdateEntity = new Lazy<GraphUpdateRootEntity>(() => ConstructRootGraphUpdateEntity());
        }

        /// <summary>
        /// Initializes a new context with a new temporary graph.
        /// </summary>
        /// <param name="graphManager">The graph manager instance.</param>
        /// <param name="graphCodifyService">The codification service for graph operations.</param>
        /// <param name="intermediateEntity">The root entity for the update.</param>
        public GraphUpdateContext(IGraphManager graphManager, IGraphCodifyService graphCodifyService, Entity intermediateEntity)
            : this()
        {
            IntermediateEntity = intermediateEntity;
            GraphCodifyService = graphCodifyService;
            GraphManager = graphManager;
            TemporaryGraph = graphManager.CreateTemporaryGraph("Stores a temporary copy of entities loaded from the materialized graph",
                "This graph is used to store a temporary copy of entities loaded from the materialized graph. " +
                "This graph is cleared after the script execution is completed."
                );
        }

        /// <summary>
        /// Initializes a new context using an existing temporary graph.
        /// </summary>
        /// <param name="temporaryGraph">The temporary graph instance.</param>
        /// <param name="graphManager">The graph manager instance.</param>
        /// <param name="graphCodifyService">The codification service for graph operations.</param>
        /// <param name="intermediateEntity">The root entity for the update.</param>
        public GraphUpdateContext(IMaterializedGraph temporaryGraph, IGraphManager graphManager, IGraphCodifyService graphCodifyService, Entity intermediateEntity)
            : this()
        {
            IntermediateEntity = intermediateEntity;
            GraphCodifyService = graphCodifyService;
            GraphManager = graphManager;
            TemporaryGraph = temporaryGraph;
        }

        private Lazy<GraphUpdateRootEntity> _rootUpdateEntity = null;

        /// <summary>
        /// Gets the root entity node for the update operation, constructed lazily.
        /// </summary>
        public GraphUpdateRootEntity EntityTreeNode => _rootUpdateEntity.Value;

        /// <summary>
        /// Constructs the root entity node for the update operation.
        /// Aggregates all relevant entities, collections, and identification paths.
        /// </summary>
        /// <returns>The constructed <see cref="GraphUpdateRootEntity"/>.</returns>
        private GraphUpdateRootEntity ConstructRootGraphUpdateEntity()
        {
            if (IntermediateEntity == null)
            {
                throw new Exception("Intermediate Entity is null");
            }

            // This needs to find all the entities in the graph from the root entity.
            // This will also need to look at collections
            // This function needs to produce the following (undefined) properties above:
            //  - List of entities that are identified exclusively by literals/uris
            //  - List of collections and the entity/property they are attached too
            //  - List of identification paths (entities that not identified by exclusively literals/uris).
            GraphUpdateRootEntity updateEntity = new GraphUpdateRootEntity(IntermediateEntity);

            return updateEntity;
        }

        /// <summary>
        /// Starts tracking changes (assertions and retractions) on the temporary graph.
        /// </summary>
        public void StartTrackChanges()
        {
            TemporaryGraph.Graph.TripleAsserted += Graph_TripleAsserted;
            TemporaryGraph.Graph.TripleRetracted += Graph_TripleRetracted;
        }

        /// <summary>
        /// Stops tracking changes on the temporary graph.
        /// </summary>
        public void StopTrackingChanges()
        {
            TemporaryGraph.Graph.TripleAsserted -= Graph_TripleAsserted;
            TemporaryGraph.Graph.TripleRetracted -= Graph_TripleRetracted;
        }

        /// <summary>
        /// Handles triple retraction events and records the retracted triple if tracking is enabled.
        /// </summary>
        private void Graph_TripleRetracted(object sender, TripleEventArgs e)
        {
            if (GraphManager.MaterializedGraph.TrackTriples)
                Retracted.Add(e.Triple);
        }

        /// <summary>
        /// Handles triple assertion events and records the asserted triple if tracking is enabled.
        /// </summary>
        private void Graph_TripleAsserted(object sender, TripleEventArgs e)
        {
            if (GraphManager.MaterializedGraph.TrackTriples)
                Asserted.Add(e.Triple);
        }

        /// <summary>
        /// Creates a duplicate context for a different entity, sharing the same temporary graph and services.
        /// </summary>
        /// <param name="existingEntity">The entity for which to duplicate the context.</param>
        /// <returns>A new <see cref="GraphUpdateContext"/> for the specified entity.</returns>
        public GraphUpdateContext DuplicateContextForEntity(Entity existingEntity)
        {
            var guc = new GraphUpdateContext(TemporaryGraph, GraphManager, GraphCodifyService, existingEntity);
            guc.ConstructRootGraphUpdateEntity();
            return guc;
        }

        /// <summary>
        /// Gets the temporary graph used for storing entities during the update operation.
        /// </summary>
        public IMaterializedGraph TemporaryGraph { get; }

        /// <summary>
        /// Gets the codification service for graph operations.
        /// </summary>
        public IGraphCodifyService GraphCodifyService { get; }
    }
}