using FEAR.Domain.KnowledgeGraph;
using FEAR.Domain.KnowledgeGraph.GraphCodify;

namespace FEAR.Domain.GraphCodifier
{
    /// <summary>
    /// Provides context for a graph codifier during execution.
    /// Used by each codifier to:
    /// <list type="bullet">
    ///   <item>Receive the data or artifact to be codified via the <see cref="Data"/> property.</item>
    ///   <item>Interact with the knowledge graph through the <see cref="GraphService"/>.</item>
    ///   <item>Notify the system of entities to return or update using <see cref="AddReturnEntity"/> and <see cref="FindOrUpdateEntity"/>.</item>
    ///   <item>Signal when codification is complete so that entities can be updated or persisted.</item>
    /// </list>
    /// </summary>
    public interface IGraphCodifierContext
    {
        /// <summary>
        /// Gets the data or artifact to be codified, provided to the codifier for processing.
        /// </summary>
        CodifierExpandoObject Data { get; }

        /// <summary>
        /// Gets the service for interacting with entities and properties.
        /// This includes creating entities as the script is execution and
        /// adding relationships between them.
        /// </summary>
        IGraphCodifyService GraphService { get; }

        /// <summary>
        /// Sets the entity that is the result of the codification process
        /// as it exists in the knowledge graph.
        /// </summary>
        /// <param name="entity">The entity to return.</param>
        void AddReturnEntity(Entity entity);

        /// <summary>
        /// Called at the end of the script execution to signal that 
        /// codification is complete and the entity can be merged into 
        /// the knowledge graph.
        /// </summary>
        /// <param name="entity">The entity to find or update.</param>
        void FindOrUpdateEntity(Entity entity);
    }
}
