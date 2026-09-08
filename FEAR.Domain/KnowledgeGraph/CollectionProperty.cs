using FEAR.Domain.KnowledgeGraph.Graphs;

namespace FEAR.Domain.KnowledgeGraph
{
    /// <summary>
    /// Represents a collection property within a knowledge graph.
    /// Encapsulates a set of elements (entities or literals), their expected type, and the owning graph context.
    /// </summary>
    public class CollectionProperty
    {
        /// <summary>
        /// Gets or sets the type of the collection.
        /// Options include rdf:List, rdf:Seq, rdf:Bag
        /// </summary>
        public string CollectionType { get; set; }

        /// <summary>
        /// Gets or sets the expected type URI for entities in the collection.
        /// If set, only entities of this type can be added.
        /// </summary>
        public Uri? ExpectedType { get; set; }

        /// <summary>
        /// Gets the graph that owns this collection property.
        /// </summary>
        public IRealGraph OwnerGraph { get; internal set; }

        /// <summary>
        /// Gets the elements of the collection. Can contain entities or literal values.
        /// </summary>
        public List<Object> Elements { get; set; } = new List<Object>();

        /// <summary>
        /// Initializes a new instance of the <see cref="CollectionProperty"/> class with a collection type and owner graph.
        /// </summary>
        /// <param name="collectionType">The type of the collection.</param>
        /// <param name="ownerGraph">The graph that owns this collection property.</param>
        public CollectionProperty(string collectionType, IRealGraph ownerGraph)
        {
            OwnerGraph = ownerGraph;
            CollectionType = collectionType;
            ExpectedType = null;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CollectionProperty"/> class with a collection type, expected type, and owner graph.
        /// </summary>
        /// <param name="collectionType">The type of the collection.</param>
        /// <param name="expectedType">The expected type (as a prefix notation string) for entities in the collection.</param>
        /// <param name="ownerGraph">The graph that owns this collection property.</param>
        /// <exception cref="Exception">Thrown if the expected type cannot be resolved to a URI.</exception>
        public CollectionProperty(string collectionType, string expectedType, IRealGraph ownerGraph)
        {
            OwnerGraph = ownerGraph;
            CollectionType = collectionType;
            var uriExpectedType = ownerGraph.GraphManager.GetUriFromPrefixNotation(expectedType);
            if(uriExpectedType.IsSuccess)
            {
                ExpectedType = uriExpectedType.Value;
            }
            else
            {
                throw new Exception(uriExpectedType.Message);
            }
        }

        /// <summary>
        /// Appends a relationship object (entity or literal) to the collection.
        /// </summary>
        /// <param name="literalObject">The object to add.</param>
        public void AppendRelationship(object literalObject)
        {
            Elements.Add(literalObject);
        }

        /// <summary>
        /// Appends a literal value to the collection.
        /// </summary>
        /// <param name="literalObject">The literal value to add.</param>
        public void AppendLiteral(object literalObject)
        {
            Elements.Add(literalObject);
        }

        /// <summary>
        /// Appends an entity to the collection, enforcing the expected type if specified.
        /// </summary>
        /// <param name="entity">The entity to add.</param>
        /// <exception cref="Exception">Thrown if the entity type does not match the expected type.</exception>
        public void AppendEntity(Entity entity)
        {
            if(ExpectedType != null)
            {
                if(entity.Type != ExpectedType)
                {
                    throw new Exception($"Expected type {ExpectedType} but got {entity.Type}");
                }
            }

            Elements.Add(entity);
        }
    }
}
