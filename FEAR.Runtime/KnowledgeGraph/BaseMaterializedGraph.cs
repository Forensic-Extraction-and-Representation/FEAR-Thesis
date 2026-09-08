using FEAR.Domain.KnowledgeGraph;
using FEAR.Domain.KnowledgeGraph.Collections;
using FEAR.Domain.KnowledgeGraph.GraphCodify;
using FEAR.Domain.KnowledgeGraph.Graphs;
using FEAR.Domain.KnowledgeGraph.GraphUpdate;
using FEAR.Domain.KnowledgeGraph.TypeService;
using System.ComponentModel;
using VDS.RDF;
using VDS.RDF.Ontology;
using VDS.RDF.Query;

namespace FEAR.Runtime.KnowledgeGraph
{
    /// <summary>
    /// Abstract base class for materialized (in-memory or persistent) knowledge graphs in FEAR.
    /// Provides core logic for entity creation, update, and property management.
    /// </summary>
    public abstract class BaseMaterializedGraph : UriBasedGraph, IMaterializedGraph
    {
        public ITypeConversionService TypeConversionService { get; }
        public IGraphCodifyServiceConfiguration Configuration { get; }

        public BaseMaterializedGraph(ITypeConversionService typeConversionService, IGraphCodifyServiceConfiguration configuration)
        {
            TypeConversionService = typeConversionService;
            Configuration = configuration;
        }

        private Lazy<string> _graphId = new Lazy<string>(() => Guid.NewGuid().ToString());
        public string GraphID => _graphId.Value;
        public string Purpose { get; set; }
        public string Description { get; set; }
        public abstract string GraphName { get; }

        public Uri CreateIRI(string indvIdentifier, string uriSegment) => GraphManager.CreateIRI(indvIdentifier, uriSegment);

        public Entity CreateEntity(string identifier, string type)
        {
            return CreateEntity(identifier, type, null);
        }

        public Entity CreateEntity(string identifier, string type, string uriSegment)
        {
            var indvIRI = CreateIRI(identifier, uriSegment);
            var indvTypeUri = GraphManager.GetUriFromPrefixNotation(type).Value;
            var indv = CreateIndividual(indvIRI, indvTypeUri);
            return new Entity()
            {
                ScriptVariableName = identifier,
                Individual = indv.Value,
                Iri = indvIRI,
                Type = indvTypeUri,
                OwnerGraph = GraphManager.MaterializedGraph,
                CustomIri = !string.IsNullOrEmpty(uriSegment)
            };
        }

        public virtual KGResponse<Individual> CreateIndividual(Uri resource, Uri @class)
        {
            var ind = Graph.CreateIndividual(resource, @class);
            return new KGResponse<Individual>().WithErrorIfNull(ind, "Failed to create the individual");
        }

        public abstract ICollectionOperationStrategy GetCollectionCreationStrategy(GraphUpdateContext context);
        public abstract ICollectionOperationStrategy GetCollectionUpdateStrategy(GraphUpdateContext context);

        /// <summary>
        /// Determines if the entity (as per the identified by clauses) already exists in the materialized graph. 
        /// If the entity already exists, it is retrieved, otherwise a new entity is created based on the identified by clauses supplied. 
        /// Once an entity in the graph is present, the entity is updated with any properties that may have been added to it, or any entity that belongs to it. 
        /// This is achieved by walking each descendant entity and looking for properties that don’t exist. 
        /// </summary>
        /// <param name="graphContext">The GraphUpdateContext that contains the root entity that was constructed</param>
        /// <returns>The created or updated entity.</returns>
        public Entity CreateOrUpdateEntity(GraphUpdateContext graphContext)
        {
            Entity targetEntity = null;
            // Attempt to find the entity in the materialized graph
            var entitySearch = GraphManager.MaterializedGraph.FindEntity(graphContext);

            // Tracking Changes will allow for an assert and retract of any triples based on creating/updating the entity
            graphContext.StartTrackChanges();

            // Use the found entity if it exists, otherwise create a new entity
            if (entitySearch.IsSuccess)
            {
                targetEntity = entitySearch.Value;
            }
            else
            {
                // Constructs an entity, handling the case where the entity is a custom IRI
                if (graphContext.IntermediateEntity.CustomIri)
                {
                    var iri = graphContext.IntermediateEntity.Iri;
                    var segment = iri.AbsolutePath;
                    var uri = iri.ToString().Substring(0, iri.ToString().Length - segment.Length);

                    targetEntity = graphContext.TemporaryGraph.CreateEntity(uri, graphContext.IntermediateEntity.Type.ToString(), segment);
                }
                else
                {
                    targetEntity = graphContext.TemporaryGraph.CreateEntity(graphContext.IntermediateEntity.Iri.Segments.Last(), graphContext.IntermediateEntity.Type.ToString());
                }
            }

            // Add or update the properties of the entity and its descendants
            graphContext.TemporaryGraph.AddOrUpdateProperties(graphContext, targetEntity, graphContext.IntermediateEntity.Properties);

            // Stop tracking changes so that the changes can be asserted and retracted
            graphContext.StopTrackingChanges();

            // Assert and retract the changes made to the graph
            GraphManager.MaterializedGraph.AssertAndRetract(new TriplesSet() { Assert = graphContext.Asserted, Retract = graphContext.Retracted });
            return targetEntity;
        }

        /// <summary>
        /// Adds or updates properties on the specified entity and recursively on its descendants.
        /// Handles all property types: entities, URIs, collections, and literals.
        /// </summary>
        /// <param name="context">The graph update context.</param>
        /// <param name="targetEntity">The entity to update.</param>
        /// <param name="properties">The set of properties to add or update.</param>
        public void AddOrUpdateProperties(GraphUpdateContext context, Entity targetEntity, PropertySet properties)
        {
            // Iterate over each property in the property set.
            foreach (PropertyValue pv in properties.AllProperties())
            {
                // The property will be related to the entity via a URI node
                Uri propertyUri = GraphManager.GetUriFromPrefixNotation(pv.PropertyName).Value;
                INode propertyNode = context.TemporaryGraph.GetNodeForUri(propertyUri);

                // Handle property value as an Entity (object property)
                if (pv.Value is Entity)
                {
                    var existingEntity = (Entity)pv.Value;

                    // If the existing entity has properties, we need to find it in the graph and update it
                    if (existingEntity.HasProperties())
                    {
                        Entity newEntity = null;

                        // Duplicate the context for the existing entity
                        var fec = context.DuplicateContextForEntity(existingEntity);

                        // We need to find the entity in the graph and then potentially update it
                        var entitySearch = GraphManager.MaterializedGraph.FindEntity(fec);
                        if (entitySearch.IsSuccess)
                        {
                            newEntity = entitySearch.Value;
                        }
                        else
                        {
                            // Ensures we check if we may have already created the entity and it has not been committed yet
                            entitySearch = context.TemporaryGraph.FindEntity(fec);
                            if (entitySearch.IsSuccess)
                            {
                                newEntity = entitySearch.Value;
                            }
                            else
                            {
                                newEntity = context.TemporaryGraph.CreateEntity(existingEntity.Iri.Segments.Last(), existingEntity.Type.ToString());
                            }
                        }

                        // Recursively add or update properties for the child entity
                        AddOrUpdateProperties(fec, newEntity, existingEntity.Properties);

                        // Only add the relationship if it doesn't already exist
                        if (targetEntity.Individual.TriplesWithSubject.WithPredicate(propertyNode).WithObject(newEntity.Individual.Resource).Count() == 0)
                        {
                            context.TemporaryGraph.AddResourceProperty(targetEntity.Individual, propertyUri, newEntity.Individual.Resource, true);
                        }
                    }
                }
                // Handle property value as a URI (object property to a resource)
                else if (pv.Value is Uri)
                {
                    var relUriNode = context.TemporaryGraph.GetNodeForUri((Uri)pv.Value);
                    if (targetEntity.Individual.TriplesWithSubject.WithPredicate(propertyNode).WithObject(relUriNode).Count() == 0)
                    {
                        context.TemporaryGraph.AddResourceProperty(targetEntity.Individual, propertyUri, relUriNode, true);
                    }
                }
                // Handle property value as a collection (rdf:List, rdf:Bag, etc.)
                else if (pv.Value is CollectionProperty)
                {
                    // If the properties are entities, we need to construct a new GraphUpdateContext for the entity
                    // We also need to load all relationships from the graph for the current entity
                    // ie, if the collection is a bag, we need to load the bag as well as rdf:_0, rdf:_1, rdf:_2, etc.
                    // If the collection is a list we need to load the list as well as rdf:first and rdf:rest

                    // Reconstruct and copy the collection to the target graph
                    var collectionProperty = (CollectionProperty)pv.Value;

                    // Try to find an existing blank node for the collection
                    var listNode = targetEntity.Individual.TriplesWithSubject.WithPredicate(propertyNode).FirstOrDefault(t => t.Object.NodeType == NodeType.Blank)?.Object as IBlankNode;
                    CollectionOperationResult cor = null;
                    // Choose the correct strategy based on whether a blank node exists
                    ICollectionOperationStrategy cos = listNode != null ? GetCollectionUpdateStrategy(context) : GetCollectionCreationStrategy(context);
                    CollectionOperationStrategyContext cosc = new CollectionOperationStrategyContext
                    {
                        Graph = context.TemporaryGraph,
                        CollectionProperty = collectionProperty,
                        TargetEntity = targetEntity,
                        PropertyUriNode = propertyNode,
                        ListNode = listNode,
                        GraphName = context.TemporaryGraph.GraphName
                    };

                    // Handle rdf:List and other collection types
                    if (collectionProperty.CollectionType == "rdf:List")
                    {
                        // This needs to be able to run the sparql query in the update directly
                        // against the endpoint. While triples can be asserted/retracted for 
                        // the in-memory one, as it will relate to blank nodes, retractions 
                        // cannot have blank nodes.
                        cor = cos.ListOperation(cosc);
                    }
                    else
                    {
                        cor = cos.BagOperation(cosc);
                    }

                    // If the operation requires SPARQL execution, temporarily disable triple tracking
                    if (cor.RequiresExecute)
                    {
                        context.TemporaryGraph.TrackTriples = false;
                        context.GraphManager.MaterializedGraph.TrackTriples = false;
                        context.TemporaryGraph.ExecuteCollectionResult(cor);
                        context.GraphManager.MaterializedGraph.ExecuteCollectionResult(cor);
                        context.TemporaryGraph.TrackTriples = true;
                        context.GraphManager.MaterializedGraph.TrackTriples = true;
                    }
                    else
                    {
                        context.TemporaryGraph.AssertAndRetract(cor.TripleSet);
                    }
                }
                // Handle property value as a literal (data property)
                else
                {
                    // Get the TypeParser for the specified type
                    object object_value = pv.Value;
                    if (pv.PropertyType.ToLower() != ConversionTypes.STRING)
                    {
                        TypeConverter converter = TypeConversionService.GetConverterForTypeUri(pv.PropertyType);

                        try
                        {
                            var entry_object_converted = converter.ConvertFromInvariantString(object_value.ToString());
                            object_value = entry_object_converted;
                        }
                        catch (Exception ex)
                        {
                            if (Configuration.StrictConversion)
                                throw new InvalidCastException($"Could not convert {object_value} to {pv.PropertyType}", ex);
                            else
                                continue;
                        }
                    }

                    // handle as a literal
                    Uri typeUri = GraphManager.GetUriFromPrefixNotation(pv.PropertyType).Value;
                    var literalNode = context.TemporaryGraph.GetOrCreateLiteralNode(object_value.ToString(), typeUri.ToString()).Value;
                    if (!targetEntity.Individual.GetLiteralProperty(propertyUri).Any(t => t.Value == literalNode.Value))
                    {
                        context.TemporaryGraph.AddLiteralProperty(targetEntity.Individual, propertyUri, literalNode, true);
                    }
                }
            }
        }

        /// <summary>
        /// Finds all subjects in the graph that have a literal value for the specified property.
        /// </summary>
        /// <param name="literal">The literal value to search for.</param>
        /// <param name="propertyUri">The property URI to search on.</param>
        /// <returns>A KGResponse containing the matching individuals.</returns>
        public virtual KGResponse<IEnumerable<Individual>> FindSubjectOfLiteral(string literal, string propertyUri)
        {
            var literalNode = Graph.GetLiteralNode(literal);
            return FindSubjectOfLiteral(literalNode, propertyUri);
        }

        /// <summary>
        /// Finds all subjects in the graph that have the specified literal node for the given property.
        /// </summary>
        /// <param name="literalNode">The literal node to search for.</param>
        /// <param name="propertyUri">The property URI to search on.</param>
        /// <returns>A KGResponse containing the matching individuals.</returns>
        public virtual KGResponse<IEnumerable<Individual>> FindSubjectOfLiteral(ILiteralNode literalNode, string propertyUri)
        {
            if (literalNode != null)
            {
                var propertyNodeUri = GraphManager.GetUriFromPrefixNotation(propertyUri).Value;
                var uriNode = GetNodeForUri(propertyNodeUri);
                var triplesWithPredicate = Graph.GetTriplesWithPredicateObject(uriNode, literalNode);
                if (triplesWithPredicate.Count() >= 1)
                {
                    var sub = triplesWithPredicate.Select(t => Graph.CreateIndividual(t.Subject));
                    return new KGResponse<IEnumerable<Individual>>().WithSuccess(sub);
                }
            }

            return new KGResponse<IEnumerable<Individual>>().WithError("Subject was not found with specified literal value");
        }

        // Abstract methods for graph operations to be implemented by concrete subclasses.
        public abstract KGResponse<IEnumerable<Individual>> FindSubjectsOfType(string type);
        public abstract KGResponse<Individual> GetIndividual(Uri resource, Uri @class);
        public abstract KGResponse<Individual> GetIndividual(IUriNode indv);
        public abstract KGResponse<ILiteralNode> GetOrCreateLiteralNode(string propertyValue, string typeUri);
        public abstract KGResponse<ILiteralNode> CreateLiteralNodeIfNotNull(string propertyValue, string dataType);
        public abstract void AddResourceProperty(Individual individual, Uri propertyUri, INode resource, bool v);
        public abstract void AddLiteralProperty(Individual individual, Uri propertyUri, ILiteralNode literalNode, bool v);
        public abstract void AddResourceProperty(Individual individual, string predicateName, INode resource, bool v);
        public abstract IBlankNode CreateBlankNode();
        public abstract void AssertAndRetract(TriplesSet triplesSet);
        public abstract object ExecuteQuery<T>(T query);
        public abstract void ExecuteUpdate<T>(T query);
        public abstract void Clear();
        public abstract int TriplesCount();
        public abstract void ExecuteCollectionResult(CollectionOperationResult cor);

        public abstract GraphStatistics GetStatistics();
    }
}
