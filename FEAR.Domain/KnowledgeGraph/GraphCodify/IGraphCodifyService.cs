using FEAR.Domain.KnowledgeGraph.Graphs;
using FEAR.Domain.KnowledgeGraph.IRI;
using VDS.RDF.Ontology;

namespace FEAR.Domain.KnowledgeGraph.GraphCodify
{
    /// <summary>
    /// Defines a contract for codifying entities, collections, and relationships in a knowledge graph.
    /// Provides methods for creating, updating, and managing ephemeral (temporary) and persistent graph objects.
    /// </summary>
    public interface IGraphCodifyService : IIRIGenerator
    {
        IEphemeralGraph EphemeralGraph { get; }

        /// <summary>
        /// Creates a temporary (ephemeral) individual in the ontology graph with the specified IRI and type.
        /// </summary>
        /// <param name="indvIRI">The IRI of the individual to create.</param>
        /// <param name="indvTypeUri">The IRI of the individual's type (class).</param>
        /// <returns>A <see cref="KGResponse{Individual}"/> containing the created individual or error information.</returns>
        KGResponse<Individual> CreateEphemeralIndividual(Uri indvIRI, Uri indvTypeUri);

        /// <summary>
        /// Resolves a URI from a prefix notation string (e.g., "rdf:type").
        /// </summary>
        /// <param name="indvType">The prefixed type string.</param>
        /// <returns>A <see cref="KGResponse{Uri}"/> containing the resolved URI or error information.</returns>
        KGResponse<Uri> GetUriFromPrefixNotation(string indvType);

        /// <summary>
        /// Creates a temporary (ephemeral) entity with the specified identifier, type, and optional URI segment.
        /// </summary>
        /// <param name="identifier">The entity's identifier.</param>
        /// <param name="type">The entity's type (as a string or URI).</param>
        /// <param name="uriSegment">An optional URI segment for the entity's IRI.</param>
        /// <returns>The created <see cref="Entity"/>.</returns>
        Entity CreateEphemeralEntity(string identifier, string type, string uriSegment = null);

        /// <summary>
        /// Creates a temporary (ephemeral) collection of the specified type.
        /// </summary>
        /// <param name="collectionType">The type of the collection (e.g., "Bag", "List").</param>
        /// <returns>The created <see cref="CollectionProperty"/>.</returns>
        CollectionProperty CreateEphemeralCollection(string collectionType);

        /// <summary>
        /// Creates a temporary (ephemeral) collection of the specified type and expected element type.
        /// </summary>
        /// <param name="collectionType">The type of the collection (e.g., "Bag", "List").</param>
        /// <param name="expectedType">The expected type of elements in the collection.</param>
        /// <returns>The created <see cref="CollectionProperty"/>.</returns>
        CollectionProperty CreateEphemeralCollection(string collectionType, string expectedType);

        /// <summary>
        /// Adds a relationship (object property) between a subject entity and an object URI.
        /// </summary>
        /// <param name="subject">The subject <see cref="Entity"/>.</param>
        /// <param name="predicateName">The predicate/property name.</param>
        /// <param name="objectUri">The object URI as a string.</param>
        void AddRelationship(Entity subject, string predicateName, string objectUri);

        /// <summary>
        /// Adds a property (literal or resource) to a subject entity.
        /// </summary>
        /// <param name="subject">The subject <see cref="Entity"/>.</param>
        /// <param name="predicateName">The predicate/property name.</param>
        /// <param name="objectType">The type of the object (e.g., datatype URI).</param>
        /// <param name="object">The property value.</param>
        void AddProperty(Entity subject, string predicateName, string objectType, object @object);

        /// <summary>
        /// Adds a property to a subject entity where the object is another entity.
        /// </summary>
        /// <param name="subject">The subject <see cref="Entity"/>.</param>
        /// <param name="predicateName">The predicate/property name.</param>
        /// <param name="object">The object <see cref="Entity"/>.</param>
        void AddProperty(Entity subject, string predicateName, Entity @object);

        /// <summary>
        /// Adds a collection as a property to a subject entity.
        /// </summary>
        /// <param name="subject">The subject <see cref="Entity"/>.</param>
        /// <param name="predicateName">The predicate/property name.</param>
        /// <param name="object">The <see cref="CollectionProperty"/> to add.</param>
        void AddCollectionAsProperty(Entity subject, string predicateName, CollectionProperty @object);

        /// <summary>
        /// Cleans up all ephemeral (temporary) entities, individuals, 
        /// and collections created during script execution.
        /// </summary>
        void CleanupEphemeral();

        /// <summary>
        /// Creates or updates a persistent entity in the graph based on the provided ephemeral entity.
        /// </summary>
        /// <param name="ephemeralEntity">The ephemeral <see cref="Entity"/> to persist or update.</param>
        /// <returns>The created or updated <see cref="Entity"/>.</returns>
        Entity CreateOrUpdateEntity(Entity ephemeralEntity);
    }
}
