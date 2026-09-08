using FEAR.Domain.KnowledgeGraph.EntitySearch;
using FEAR.Domain.KnowledgeGraph.GraphManager;
using FEAR.Domain.KnowledgeGraph.Graphs;
using FEAR.Domain.KnowledgeGraph.GraphUpdate;
using FEAR.Domain.KnowledgeGraph.IRI;
using FEAR.Domain.KnowledgeGraph.TypeService;
using System.ComponentModel;
using VDS.RDF;
using VDS.RDF.Ontology;

namespace FEAR.Domain.KnowledgeGraph.GraphCodify
{
    /// <summary>
    /// Provides services for codifying domain objects into a knowledge graph.
    /// Handles creation of individuals, entities, collections, and relationships in an ephemeral (temporary) graph
    /// and supports property/relationship management, IRI generation, and entity materialization.
    /// </summary>
    public class GraphCodifyService : IGraphCodifyService
    {
        // Delegate for creating individuals in the graph.
        delegate KGResponse<Individual> CreateIndividualDelegate(Uri identifier, Uri type);

        // Manages access to graphs and ontology.
        IGraphManager GraphManager { get; }
        // Configuration options for codification.
        IGraphCodifyServiceConfiguration Configuration { get; }
        // Handles type conversions for property values.
        ITypeConversionService TypeConversionService { get; }
        // Ephemeral (temporary) graph for script/session-scoped codification.
        public IEphemeralGraph EphemeralGraph { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="GraphCodifyService"/> class.
        /// </summary>
        /// <param name="findEntityStrategyFactoryManager">Factory for entity finding strategies.</param>
        /// <param name="graphCodifyServiceConfig">Codification configuration.</param>
        /// <param name="graphManager">Graph manager for ontology/materialized/ephemeral graphs.</param>
        /// <param name="typeConversionService">Type conversion service for property values.</param>
        public GraphCodifyService(
            FindEntityStrategyFactoryManager findEntityStrategyFactoryManager,
            IGraphCodifyServiceConfiguration graphCodifyServiceConfig,
            IGraphManager graphManager,
            ITypeConversionService typeConversionService)
        {
            Configuration = graphCodifyServiceConfig;
            TypeConversionService = typeConversionService;
            GraphManager = graphManager;
            EphemeralGraph = GraphManager.CreateTemporaryGraph("Stores the knowledge graph representation of an encoded artifact within a script",
                "Ephemeral graph used to store the knowledge graph representation of an encoded artifact within a script. " +
                "This graph is cleared after the script execution is completed.");
        }

        /// <summary>
        /// Creates an IRI for an individual using the graph manager.
        /// </summary>
        public Uri CreateIRI(string indvIdentifier, string uriSegment) => GraphManager.CreateIRI(indvIdentifier, uriSegment);

        /// <summary>
        /// Resolves a URI from a prefix notation (e.g., "rdf:Type") using the graph manager.
        /// </summary>
        public KGResponse<Uri> GetUriFromPrefixNotation(string indvType) => GraphManager.GetUriFromPrefixNotation(indvType);

        /// <summary>
        /// Creates an individual in the ephemeral graph with the specified IRI and type.
        /// </summary>
        public KGResponse<Individual> CreateEphemeralIndividual(Uri indvIRI, Uri indvTypeUri)
        {
            return EphemeralGraph.CreateIndividual(indvIRI, indvTypeUri);
        }

        /// <summary>
        /// Creates an ephemeral entity (script/session-scoped) with the specified identifier, type, and optional URI segment.
        /// </summary>
        public Entity CreateEphemeralEntity(string identifier, string type, string uriSegment = null)
        {
            // Create the IRI for the individual
            var indvIRI = CreateIRI(identifier, uriSegment);

            // Create the Uri for the type of the entity
            var indvTypeUri = GetUriFromPrefixNotation(type).Value;

            // Create a new individual in the ephemeral graph with the specified IRI and type
            var indv = CreateEphemeralIndividual(indvIRI, indvTypeUri);

            return new Entity() { 
                ScriptVariableName = identifier, 
                Individual = indv.Value, 
                Iri = indvIRI, 
                Type = indvTypeUri, 
                OwnerGraph = EphemeralGraph, 
                CustomIri = !string.IsNullOrEmpty(uriSegment) 
            };
        }

        /// <summary>
        /// Creates an ephemeral collection property of the specified type.
        /// </summary>
        public CollectionProperty CreateEphemeralCollection(string collectionType)
        {
            return new CollectionProperty(collectionType, EphemeralGraph);
        }

        /// <summary>
        /// Creates an ephemeral collection property of the specified type and expected element type.
        /// </summary>
        public CollectionProperty CreateEphemeralCollection(string collectionType, string expectedType)
        {
            return new CollectionProperty(collectionType, expectedType, EphemeralGraph);
        }

        /// <summary>
        /// Adds a relationship (object property) between the subject entity and an object URI.
        /// </summary>
        public void AddRelationship(Entity subject, string predicateName, string objectUri)
        {
            var og = GraphManager.OntologyGraph;
            var oName = og.ResolveQName(objectUri);

            subject.AddProperty(predicateName, "RELATIONSHIP", oName);
            var uriNode = EphemeralGraph.CreateUriNode(oName);
            EphemeralGraph.AddResourceProperty(subject.Individual, predicateName, uriNode, true);
        }

        /// <summary>
        /// Adds a property (data property) to the subject entity, inferring the type if not provided.
        /// </summary>
        public void AddProperty(Entity subject, string predicateName, string objectType, object @object)
        {
            object value = @object;
            if (@object is NullingExpandoObject)
                value = (@object as NullingExpandoObject)["_VALUE"];

            // Ignore null or empty values
            if (value == null || (value is string && String.IsNullOrEmpty(value as string)))
                return;

            // If the object type is not specified, try to infer it from the ontology graph
            if (String.IsNullOrEmpty(objectType))
            {
                // Attempt to get the range of the predicate from the ontology graph
                var og = GraphManager.OntologyGraph;
                var pName = og.ResolveQName(predicateName);
                var range = og.ResolveQName("rdfs:range");
                var pNameNode = og.GetUriNode(pName);
                var rangeNode = og.GetUriNode(range);

                // Try to infer the type of the object from the ontology graph
                if (rangeNode != null && pNameNode != null)
                {
                    var rangeTriplesForObject = og.GetTriplesWithSubjectPredicate(pNameNode, rangeNode).SingleOrDefault();

                    if (rangeTriplesForObject != null)
                    {
                        objectType = (rangeTriplesForObject.Object as IUriNode).Uri.ToString();
                    }
                    else
                    {
                        throw new InvalidEnumArgumentException($"Could not determine the type of the object for the property {predicateName}");
                    }
                }
            }

            // Add the property to the subject and the individual of the subject
            subject.AddProperty(predicateName, objectType, value);
            LiteralDefinitionBuilder ldb = LiteralDefinitionBuilder.CreateWithLiteral(predicateName, value, objectType);

            // Add the literal property to the ephemeral graph
            ldb.Build(EphemeralGraph).ToList().ForEach((literalEntry) =>
            {
                EphemeralGraph.AddLiteralProperty(subject.Individual, literalEntry.Key, literalEntry.Value, true);
            });
        }

        /// <summary>
        /// Adds a property (object property) to the subject entity, linking to another entity.
        /// </summary>
        public void AddProperty(Entity subject, string predicateName, Entity @object)
        {
            if (@object == null || @object.IdentifiedByOptions.Count == 0)
                return;

            // Add the property to the subject and the individual of the subject
            subject.AddProperty(predicateName, "Entity", @object);

            // Add the relationship to the ephemeral graph
            EphemeralGraph.AddResourceProperty(subject.Individual, predicateName, @object.Individual.Resource, true);
        }

        /// <summary>
        /// Adds a collection property to the subject entity.
        /// </summary>
        public void AddCollectionAsProperty(Entity subject, string predicateName, CollectionProperty collectionProperty)
        {
            subject.AddProperty(predicateName, collectionProperty.CollectionType, collectionProperty);
        }

        /// <summary>
        /// Clears all nodes from the ephemeral graph, resetting its state.
        /// </summary>
        public void CleanupEphemeral()
        {
            // Remove all nodes of the ephemeral graph
            //EphemeralGraph.Graph.Clear();
        }

        /// <summary>
        /// Creates or updates an entity in the materialized graph using the provided intermediate entity.
        /// </summary>
        public Entity CreateOrUpdateEntity(Entity intermediateEntity)
        {
            GraphUpdateContext context = new GraphUpdateContext(GraphManager, this, intermediateEntity);
            return GraphManager.MaterializedGraph.CreateOrUpdateEntity(context);
        }

        /// <summary>
        /// Resets the IRI generator state.
        /// </summary>
        void IIRIGenerator.Reset()
        {

        }
    }
}
