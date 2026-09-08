using System.Text;

namespace FEAR.Domain.KnowledgeGraph.GraphUpdate
{
    /// <summary>
    /// Represents a collection node in the graph update structure.
    /// Handles parsing of collection elements into graph update nodes (entities or literals),
    /// and computes a hash representing the collection's contents.
    /// </summary>
    public class GraphUpdateCollection : GraphUpdateNode
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GraphUpdateCollection"/> class.
        /// Parses the provided collection into graph update nodes and computes the collection hash.
        /// </summary>
        /// <param name="collection">The collection property containing the elements.</param>
        /// <param name="queryIdentifier">A unique identifier for the collection in queries.</param>
        /// <param name="graphUpdateEntityParentReference">Reference to the parent entity and property.</param>
        public GraphUpdateCollection(CollectionProperty collection, string queryIdentifier, GraphUpdateEntityParentReference graphUpdateEntityParentReference)
        {
            ParentReference = graphUpdateEntityParentReference;
            CollectionQueryIdentifier = queryIdentifier;
            Collection = collection;

            ParseCollection();
            Hash = ComputeHash();
        }

        /// <summary>
        /// The elements of the collection, represented as graph update nodes (entities or literals).
        /// </summary>
        public List<GraphUpdateNode> Elements { get; protected set; } = new List<GraphUpdateNode>();

        /// <summary>
        /// Gets or sets the unique identifier for the collection in queries.
        /// </summary>
        public string CollectionQueryIdentifier { get; set; }

        /// <summary>
        /// Gets or sets the collection property containing the elements.
        /// </summary>
        public CollectionProperty Collection { get; set; }

        /// <summary>
        /// Computes a hash representing the contents of the collection.
        /// The hash is based on the hashes or values of the collection elements.
        /// </summary>
        /// <returns>A base64-encoded string representing the collection hash.</returns>
        private string ComputeHash()
        {
            List<string> components = new List<string>();
            foreach (var element in Elements)
            {
                if (element is GraphUpdateEntity)
                {
                    components.Add((element as GraphUpdateEntity).Hash);
                }
                else if (element is GraphUpdateLiteral)
                {
                    components.Add((element as GraphUpdateLiteral).NodeValue.Value.ToString());
                }
            }

            return Convert.ToBase64String(sha256.ComputeHash(Encoding.UTF8.GetBytes(string.Join("", components))));
        }

        /// <summary>
        /// Parses the collection property and populates the <see cref="Elements"/> list
        /// with graph update nodes for each element (entity or literal).
        /// </summary>
        public void ParseCollection()
        {
            int i = 0;
            foreach (var element in Collection.Elements)
            {
                if (element is Entity)
                {
                    Elements.Add(new GraphUpdateEntity(element as Entity, CollectionQueryIdentifier + $"_c{i++}", ParentReference, IdentifiedByCondition.None));
                }
                else
                {
                    Elements.Add(new GraphUpdateLiteral(new PropertyValue() { PropertyName = "", Value = element, PropertyType = Collection.CollectionType }, ParentReference, IdentifiedByCondition.None));
                }
            }
        }

        /// <summary>
        /// Adds a new element to the collection if it is a valid graph update entity or literal.
        /// </summary>
        /// <param name="node">The node to add to the collection.</param>
        public void AddElement(GraphUpdateNode node)
        {
            if (node is GraphUpdateEntity || node is GraphUpdateLiteral)
                Elements.Add(node);
        }
    }
}