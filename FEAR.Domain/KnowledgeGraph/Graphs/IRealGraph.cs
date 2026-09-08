using FEAR.Domain.KnowledgeGraph.GraphManager;
using FEAR.Domain.KnowledgeGraph.GraphUpdate;
using VDS.RDF;
using VDS.RDF.Ontology;
using VDS.RDF.Query;

namespace FEAR.Domain.KnowledgeGraph.Graphs
{
    /// <summary>
    /// Represents a real (materialized or remote) graph in the knowledge graph system.
    /// Provides methods for entity management, IRI creation, property assertion, and SPARQL operations.
    /// Inherits from <see cref="IUriBasedGraph"/> for URI and node management.
    /// </summary>
    public interface IRealGraph : IUriBasedGraph
    {
        /// <summary>
        /// Gets or sets the graph manager responsible for this graph instance.
        /// </summary>
        IGraphManager GraphManager { get; set; }

        /// <summary>
        /// Gets the name of the graph.
        /// </summary>
        string GraphName { get; }

        /// <summary>
        /// Gets or sets the purpose of the graph (e.g., for documentation or context).
        /// </summary>
        string Purpose { get; set; }

        /// <summary>
        /// Gets or sets a description of the graph.
        /// </summary>
        string Description { get; set; }

        /// <summary>
        /// Finds an entity in the graph using the provided update context which contains
        /// identifying properties for the entity.
        /// </summary>
        /// <param name="context">The update context containing search parameters.</param>
        /// <returns>A <see cref="KGResponse{Entity}"/> containing the found entity or error information.</returns>
        KGResponse<Entity> FindEntity(GraphUpdateContext context);

        /// <summary>
        /// Finds all subjects (individuals) of the specified type.
        /// </summary>
        /// <param name="type">The type URI or name as a string.</param>
        /// <returns>A <see cref="KGResponse{IEnumerable{Individual}}"/> containing the found subjects.</returns>
        KGResponse<IEnumerable<Individual>> FindSubjectsOfType(string type);

        /// <summary>
        /// Creates a new IRI for an individual, using the specified identifier and URI segment.
        /// </summary>
        /// <param name="indvIdentifier">The identifier for the individual.</param>
        /// <param name="uriSegment">A custom segment for the IRI.</param>
        /// <returns>The created <see cref="Uri"/>.</returns>
        Uri CreateIRI(string indvIdentifier, string uriSegment);

        /// <summary>
        /// Finds all subjects (individuals) that have a literal value for a given property.
        /// </summary>
        /// <param name="literal">The literal value to search for.</param>
        /// <param name="propertyUri">The property URI as a string.</param>
        /// <returns>A <see cref="KGResponse{IEnumerable{Individual}}"/> containing the found subjects.</returns>
        KGResponse<IEnumerable<Individual>> FindSubjectOfLiteral(string literal, string propertyUri);

        /// <summary>
        /// Finds all subjects (individuals) that have a specific literal node for a given property.
        /// </summary>
        /// <param name="literalNode">The literal node to search for.</param>
        /// <param name="propertyUri">The property URI as a string.</param>
        /// <returns>A <see cref="KGResponse{IEnumerable{Individual}}"/> containing the found subjects.</returns>
        KGResponse<IEnumerable<Individual>> FindSubjectOfLiteral(ILiteralNode literalNode, string propertyUri);

        /// <summary>
        /// Creates or updates an entity in the graph using the provided update context.
        /// This will use the GraphUpdateContext to determine if the entity already exists,
        /// and will ensure that all entities and properties are correctly created or updated
        /// in the "identified by" path for the entity.
        /// </summary>
        /// <param name="fec">The update context for the entity.</param>
        /// <returns>The created or updated <see cref="Entity"/>.</returns>
        Entity CreateOrUpdateEntity(GraphUpdateContext fec);

        /// <summary>
        /// Creates a new entity in the graph with the specified identifier and type.
        /// </summary>
        /// <param name="identifier">The entity's identifier.</param>
        /// <param name="type">The entity's type.</param>
        /// <returns>The created <see cref="Entity"/>.</returns>
        Entity CreateEntity(string identifier, string type);

        /// <summary>
        /// Creates a new entity in the graph with the specified identifier, type, and URI segment.
        /// </summary>
        /// <param name="identifier">The entity's identifier.</param>
        /// <param name="type">The entity's type.</param>
        /// <param name="uriSegment">A custom segment for the entity's IRI.</param>
        /// <returns>The created <see cref="Entity"/>.</returns>
        Entity CreateEntity(string identifier, string type, string uriSegment);

        /// <summary>
        /// Creates a new individual in the graph with the specified resource and class URIs.
        /// </summary>
        /// <param name="resource">The resource URI for the individual.</param>
        /// <param name="class">The class URI for the individual's type.</param>
        /// <returns>A <see cref="KGResponse{Individual}"/> containing the created individual.</returns>
        KGResponse<Individual> CreateIndividual(Uri resource, Uri @class);

        /// <summary>
        /// Gets an individual from the graph by resource and class URIs.
        /// </summary>
        /// <param name="resource">The resource URI for the individual.</param>
        /// <param name="class">The class URI for the individual's type.</param>
        /// <returns>A <see cref="KGResponse{Individual}"/> containing the found individual.</returns>
        KGResponse<Individual> GetIndividual(Uri resource, Uri @class);

        /// <summary>
        /// Gets an individual from the graph by its URI node.
        /// </summary>
        /// <param name="indv">The URI node of the individual.</param>
        /// <returns>A <see cref="KGResponse{Individual}"/> containing the found individual.</returns>
        KGResponse<Individual> GetIndividual(IUriNode indv);

        /// <summary>
        /// Gets or creates a literal node for the specified property value and type URI.
        /// </summary>
        /// <param name="propertyValue">The value of the property.</param>
        /// <param name="typeUri">The type URI of the property.</param>
        /// <returns>A <see cref="KGResponse{ILiteralNode}"/> containing the literal node.</returns>
        KGResponse<ILiteralNode> GetOrCreateLiteralNode(string propertyValue, string typeUri);

        /// <summary>
        /// Creates a literal node if the property value is not null.
        /// </summary>
        /// <param name="propertyValue">The value of the property.</param>
        /// <param name="dataType">The data type URI of the property.</param>
        /// <returns>A <see cref="KGResponse{ILiteralNode}"/> containing the literal node.</returns>
        KGResponse<ILiteralNode> CreateLiteralNodeIfNotNull(string propertyValue, string dataType);

        /// <summary>
        /// Adds a resource property (object property) to an individual.
        /// </summary>
        /// <param name="individual">The subject individual.</param>
        /// <param name="propertyUri">The property URI.</param>
        /// <param name="resource">The object resource node.</param>
        /// <param name="v">Indicates if the property should be asserted (true) or retracted (false).</param>
        void AddResourceProperty(Individual individual, Uri propertyUri, INode resource, bool v);

        /// <summary>
        /// Adds a literal property (data property) to an individual.
        /// </summary>
        /// <param name="individual">The subject individual.</param>
        /// <param name="propertyUri">The property URI.</param>
        /// <param name="literalNode">The literal node to add.</param>
        /// <param name="v">Indicates if the property should be asserted (true) or retracted (false).</param>
        void AddLiteralProperty(Individual individual, Uri propertyUri, ILiteralNode literalNode, bool v);

        /// <summary>
        /// Adds a resource property (object property) to an individual using a predicate name.
        /// </summary>
        /// <param name="individual">The subject individual.</param>
        /// <param name="predicateName">The predicate/property name.</param>
        /// <param name="resource">The object resource node.</param>
        /// <param name="v">Indicates if the property should be asserted (true) or retracted (false).</param>
        void AddResourceProperty(Individual individual, string predicateName, INode resource, bool v);

        /// <summary>
        /// Creates a new blank node in the graph.
        /// </summary>
        /// <returns>The created <see cref="IBlankNode"/>.</returns>
        IBlankNode CreateBlankNode();

        /// <summary>
        /// Asserts and/or retracts triples in the graph as specified by the given set.
        /// </summary>
        /// <param name="triplesSet">The set of triples to assert or retract.</param>
        void AssertAndRetract(TriplesSet triplesSet);

        /// <summary>
        /// Executes a SPARQL query against the graph.
        /// </summary>
        /// <param name="query">The SPARQL query to execute.</param>
        /// <returns>The result of the query, which may be a result set or other object.</returns>
        object ExecuteQuery<T>(T query);

        /// <summary>
        /// Executes a SPARQL update command against the graph.
        /// </summary>
        /// <param name="query">The SPARQL update command to execute.</param>
        void ExecuteUpdate<T>(T query);
    }
}
