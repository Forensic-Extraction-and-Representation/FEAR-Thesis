using FEAR.Domain.KnowledgeGraph.EntitySearch;
using System.Text;
using HashStr = System.String;

namespace FEAR.Domain.KnowledgeGraph.GraphUpdate
{
    /// <summary>
    /// Represents an entity node in the graph update structure.
    /// Handles parsing of entity properties into child nodes, hash computation,
    /// and provides methods to retrieve leaf entities, collections, and identified entities.
    /// </summary>
    public class GraphUpdateEntity : GraphUpdateIdentifiableNode
    {
        /// <summary>
        /// The identifier used in queries for this entity.
        /// </summary>
        public string EntityQueryIdentifier { get; set; }

        /// <summary>
        /// The child nodes of this entity, which may include other entities, collections, or literals.
        /// </summary>
        public List<GraphUpdateNode> Children { get; set; }

        /// <summary>
        /// The underlying entity represented by this node.
        /// </summary>
        public Entity Entity { get; set; }

        /// <summary>
        /// Indicates if this entity is a leaf (contains only literals, no child entities or collections).
        /// </summary>
        public bool IsLeafEntity => (LiteralsCount.Required > 0) && EntitiesCount.Counts(t => t != IdentifiedByCondition.None).All(t => t == 0);

        /// <summary>
        /// Indicates if this entity is the root of the update structure.
        /// </summary>
        public bool IsRootEntity => ParentReference?.ParentEntity == null;

        /// <summary>
        /// Gets the child nodes that identify the parent entity by a specific condition.
        /// </summary>
        public IList<GraphUpdateIdentifiableNode> IdentifyingNodes => Children.OfType<GraphUpdateIdentifiableNode>()
            .Where(t => t.IdentifiesParentEntityByCondition != IdentifiedByCondition.None)
            .ToList();

        /// <summary>
        /// Counter for literal properties grouped by identification condition.
        /// </summary>
        public PropertyCounter LiteralsCount { get; set; } = new PropertyCounter();

        /// <summary>
        /// Counter for entity properties grouped by identification condition.
        /// </summary>
        public PropertyCounter EntitiesCount { get; set; } = new PropertyCounter();

        /// <summary>
        /// Counter for collection properties grouped by identification condition.
        /// </summary>
        public PropertyCounter CollectionsCount { get; set; } = new PropertyCounter();

        /// <summary>
        /// Returns a string representation of the entity, including its identifier and identifying nodes.
        /// </summary>
        public override string ToString()
        {
            string identifiedBy = "";
            if (IdentifyingNodes.Any())
            {
                identifiedBy = string.Join(", ", IdentifyingNodes.Select(t => t.ToString()));
            }

            return $"{EntityQueryIdentifier}: [IdentifiedBy {identifiedBy}]";
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GraphUpdateEntity"/> class.
        /// Parses the entity's properties into child nodes and computes its hash.
        /// </summary>
        /// <param name="ent">The entity to represent.</param>
        /// <param name="queryIdentifierStub">The identifier stub for queries.</param>
        /// <param name="parentReference">Reference to the parent entity and property.</param>
        /// <param name="identifiesParentByCondition">The identification condition for the parent.</param>
        public GraphUpdateEntity(Entity ent, string queryIdentifierStub, GraphUpdateEntityParentReference parentReference, IdentifiedByCondition identifiesParentByCondition)
        {
            Children = new List<GraphUpdateNode>();

            Entity = ent;
            EntityQueryIdentifier = queryIdentifierStub;
            ParentReference = parentReference;
            IdentifiesParentEntityByCondition = identifiesParentByCondition;

            ParseEntity();
            Hash = ComputeHash();
        }

        /// <summary>
        /// Parses the entity's properties and creates child nodes for entities, collections, and literals.
        /// Updates property counters accordingly.
        /// </summary>
        private void ParseEntity()
        {
            int queryIdentifierCounter = 0;

            // Iterate through all properties of the entity and create child nodes based on their types.
            foreach (var prop in Entity.Properties.AllProperties())
            {
                IdentifiedByOption ibo = Entity.GetIdentifiedByOption(prop.PropertyName);
                var entityPropertyAsParent = new GraphUpdateEntityParentReference(this, prop.PropertyName);

                if (prop.Value is Entity)
                {
                    // If the property is an entity, create a GraphUpdateEntity node for it.
                    EntitiesCount.AddCount(ibo.IdentifiedByCondition);
                    var entity = prop.Value as Entity;
                    Children.Add(new GraphUpdateEntity(entity, $"{EntityQueryIdentifier}_{queryIdentifierCounter++}", entityPropertyAsParent, ibo.IdentifiedByCondition));
                }
                else if (prop.Value is CollectionProperty)
                {
                    // If the property is a collection, create a GraphUpdateCollection node for it.
                    CollectionsCount.AddCount(ibo.IdentifiedByCondition);
                    var collection = prop.Value as CollectionProperty;
                    Children.Add(new GraphUpdateCollection(collection, $"{EntityQueryIdentifier}_COL{queryIdentifierCounter++}", entityPropertyAsParent));
                }
                else
                {
                    // If the property is a literal, create a GraphUpdateLiteral node for it.
                    LiteralsCount.AddCount(ibo.IdentifiedByCondition);
                    Children.Add(new GraphUpdateLiteral(new PropertyValue()
                    {
                        PropertyName = prop.PropertyName,
                        Value = prop.Value,
                        PropertyType = prop.PropertyType
                    }, entityPropertyAsParent, ibo.IdentifiedByCondition));
                }
            }
        }

        /// <summary>
        /// Computes the hash of the entity based on its children and their hashes.
        /// </summary>
        /// <returns>A base64 encoded string representing the hash of the entity.</returns>
        private string ComputeHash()
        {
            List<string> components = new List<string>();
            foreach (var component in Children)
            {
                components.Add(component.ParentReference.PropertyName);
                components.Add(component.Hash);
            }

            return Convert.ToBase64String(sha256.ComputeHash(Encoding.UTF8.GetBytes(string.Join("", components))));
        }

        /// <summary>
        /// Recursively collects all leaf entities (entities with only literals) in the subtree.
        /// This will also include 'loose' entities, which are entities that connected to the 
        /// root entity as part of collections.
        /// </summary>
        /// <returns>A dictionary of leaf entities keyed by their hash.</returns>
        protected IDictionary<HashStr, GraphUpdateEntity> ReturnLeafEntities()
        {
            IDictionary<HashStr, GraphUpdateEntity> leafEntities = new Dictionary<HashStr, GraphUpdateEntity>();

            // If this entity is a leaf, add it to the dictionary.
            if (IsLeafEntity && !leafEntities.ContainsKey(Hash))
                leafEntities.Add(Hash, this);
            else
            {
                // If this entity is not a leaf, recursively collect leaf entities from its children.
                foreach (var child in Children)
                {
                    if (child is GraphUpdateEntity)
                    {
                        GetLeafEntitiesFromChild(leafEntities, child);
                    }
                    else if (child is GraphUpdateCollection)
                    {
                        // This will collect what is known as loose entities.
                        // If the child is a collection, iterate through its elements and collect leaf entities.
                        foreach (var element in (child as GraphUpdateCollection).Elements)
                        {
                            if (element is GraphUpdateEntity)
                            {
                                GetLeafEntitiesFromChild(leafEntities, element);
                            }
                        }
                    }
                }
            }

            return leafEntities;

            /// <summary>
            /// Recursively collects leaf entities from a child node.
            /// </summary>
            static void GetLeafEntitiesFromChild(IDictionary<string, GraphUpdateEntity> leafEntities, GraphUpdateNode child)
            {
                var entity = child as GraphUpdateEntity;
                if (entity.IsLeafEntity)
                {
                    // If the child is a leaf entity, add it to the dictionary if not already present.
                    if (!leafEntities.ContainsKey(entity.Hash))
                        leafEntities.Add(entity.Hash, entity);
                }
                else
                {
                    // If the child is not a leaf, collect its leaf entities recursively.
                    var x = entity.ReturnLeafEntities();
                    foreach (var kvp in x)
                    {
                        if (!leafEntities.ContainsKey(kvp.Key))
                            leafEntities.Add(kvp.Key, kvp.Value);
                    }
                }
            }
        }

        /// <summary>
        /// Recursively collects all collection nodes in the subtree.
        /// </summary>
        /// <returns>A list of all <see cref="GraphUpdateCollection"/> nodes.</returns>
        protected IList<GraphUpdateCollection> ReturnCollections()
        {
            IList<GraphUpdateCollection> collections = new List<GraphUpdateCollection>();

            foreach (var child in Children)
            {
                // If the child is a collection, add it to the list.
                if (child is GraphUpdateCollection)
                {
                    collections.Add(child as GraphUpdateCollection);
                }
                else if (child is GraphUpdateEntity)
                {
                    // If the child is an entity, recursively collect collections from it.
                    var x = (child as GraphUpdateEntity).ReturnCollections();
                    if (x.Any())
                        collections = collections.Concat(x).ToList();
                }
            }

            return collections;
        }

        /// <summary>
        /// Recursively collects all entities in the subtree, excluding leaf entities, 
        /// that are identified by a property.
        /// </summary>
        /// <param name="depth">The current depth in the tree.</param>
        /// <param name="idEntities">The dictionary to populate with identifying entities.</param>
        protected void ReturnIdentifyiedEntities(int depth, ref IDictionary<HashStr, IdentifiedEntity> idEntities)
        {
            // This may not be needed as the entity should be added by its parent.
            // If this entity is identified by a condition and not already in the dictionary, add it.
            if (!idEntities.ContainsKey(Hash) && IdentifyingNodes.Any())
            {
                idEntities.Add(Hash, new IdentifiedEntity() { Entity = this, Depth = depth });
            }
            //*/

            foreach (var child in Children)
            {
                if (child is GraphUpdateCollection)
                {
                    // If the child is a collection, iterate through its elements and collect identified entities.
                    foreach (var element in (child as GraphUpdateCollection).Elements)
                    {
                        if (element is GraphUpdateEntity)
                        {
                            // If the element is a GraphUpdateEntity, recursively collect identified entities from it.
                            GraphUpdateEntity gucChild = element as GraphUpdateEntity;
                            if (!gucChild.IsLeafEntity)
                            {
                                gucChild.ReturnIdentifyiedEntities(depth + 1, ref idEntities);
                            }
                        }
                    }
                }
                else if (child is GraphUpdateEntity)
                {
                    // If the child is a GraphUpdateEntity, recursively collect identified entities from it.

                    GraphUpdateEntity gucChild = child as GraphUpdateEntity;
                    // Leafs are already included in the ReturnLeafEntities function and not needed
                    if (!gucChild.IsLeafEntity)
                    {
                        gucChild.ReturnIdentifyiedEntities(depth + 1, ref idEntities);
                    }
                }
            }
        }
    }
}