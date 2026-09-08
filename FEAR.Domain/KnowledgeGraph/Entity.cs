using FEAR.Domain.KnowledgeGraph.Graphs;
using System.Security.Cryptography;
using System.Text;
using VDS.RDF;
using VDS.RDF.Ontology;

namespace FEAR.Domain.KnowledgeGraph
{
    /// <summary>
    /// Represents an entity (node) in the knowledge graph.
    /// Encapsulates properties, type, IRI, and identification logic for graph-based data modeling.
    /// </summary>
    public class Entity
    {
        /// <summary>
        /// Shared SHA256 instance for computing entity hashes.
        /// </summary>
        private static SHA256 sha256 = SHA256.Create();

        /// <summary>
        /// Gets the set of properties associated with this entity.
        /// </summary>
        public PropertySet Properties { get; protected set; } = new PropertySet();

        /// <summary>
        /// Gets the list of identification options for this entity.
        /// Used to determine how the entity is uniquely identified in the graph.
        /// </summary>
        public List<IdentifiedByOption> IdentifiedByOptions { get; protected set; } = new List<IdentifiedByOption>();

        /// <summary>
        /// Gets or sets the IRI (Internationalized Resource Identifier) of the entity.
        /// </summary>
        public Uri Iri { get; set; }

        /// <summary>
        /// Gets or sets the variable name used in the script that generated the entity.
        /// </summary>
        public string ScriptVariableName { get; set; }

        /// <summary>
        /// Gets or sets the type/class URI of the entity.
        /// </summary>
        public Uri Type { get; set; }

        /// <summary>
        /// Gets or sets the underlying ontology individual associated with this entity.
        /// </summary>
        public Individual Individual { get; set; }

        /// <summary>
        /// Gets or sets the graph that this entity is included in.
        /// </summary>
        public IRealGraph OwnerGraph { get; set; }

        /// <summary>
        /// Indicates whether the IRI was set via a URI formatted string in the script.
        /// </summary>
        public bool CustomIri { get; set; } = false;

        /// <summary>
        /// Stores the computed hash for this entity.
        /// </summary>
        private string EntityHash { get; set; }

        /// <summary>
        /// Returns a string representation of the entity and its properties, recursively.
        /// Useful for debugging and visualization.
        /// </summary>
        /// <param name="depth">The indentation depth for nested entities.</param>
        /// <returns>A formatted string representing the entity structure.</returns>
        public string EntityView(int depth)
        {
            StringBuilder sb = new StringBuilder();
            string indent = new string(' ', 2 * depth);

            sb.AppendLine($"{indent}Entity: {Iri} ({Type}) {{");
            indent = new string(' ', 2 * (depth + 1));

            foreach (var prop in Properties.AllProperties())
            {
                IdentifiedByOption identifiedByOption = GetIdentifiedByOption(prop.PropertyName);
                if (prop.PropertyType == "Entity")
                {
                    var entValue = prop.Value as Entity;
                    var condition = identifiedByOption?.Entity.ScriptVariableName == entValue.ScriptVariableName ? identifiedByOption.IdentifiedByCondition.ToString() : "None";
                    sb.AppendLine($"{indent}{prop.PropertyName} [{condition}] => ");
                    sb.AppendLine($"{entValue.EntityView(depth + 1)}");
                }
                else if (prop.Value is CollectionProperty)
                {
                    sb.AppendLine($"{indent}{prop.PropertyName} [Collection] => {{");
                    foreach (var item in ((CollectionProperty)prop.Value).Elements)
                    {
                        if (item is Entity entityItem)
                        {
                            sb.AppendLine($"{entityItem.EntityView(depth + 2)}");
                        }
                        else
                        {
                            sb.AppendLine($"{indent}{item.ToString()}");
                        }
                    }
                    sb.AppendLine($"{indent}}}");
                }
                else
                {
                    var condition = identifiedByOption?.Property == prop.PropertyName ? identifiedByOption.IdentifiedByCondition.ToString() : "None";
                    sb.AppendLine($"{indent}{prop.PropertyName} [{condition}] {prop.Value}");
                }
            }

            indent = new string(' ', 2 * depth);
            sb.AppendLine($"{indent}}}");
            return sb.ToString();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Entity"/> class and computes its hash.
        /// </summary>
        public Entity()
        {
            List<string> components = new List<string>();

            foreach (var x in Properties.AllProperties())
            {
                if (x.Value is Entity)
                {
                    components.Add((x.Value as Entity).EntityHash);
                }
                else if (x.Value is CollectionProperty)
                {
                    (x.Value as CollectionProperty).Elements.ForEach(e =>
                    {
                        if (e is Entity)
                            components.Add((e as Entity).EntityHash);
                        else
                            components.Add(e.ToString());
                    });
                }
                else
                {
                    components.Add(x.Value.ToString());
                }
            }

            EntityHash = Convert.ToBase64String(sha256.ComputeHash(Encoding.UTF8.GetBytes(string.Join("", components))));
        }

        /// <summary>
        /// Retrieves the identification option for a given property name.
        /// Returns a default option if none is found.
        /// </summary>
        /// <param name="propertyName">The property name to check.</param>
        /// <returns>The matching <see cref="IdentifiedByOption"/> or a default option.</returns>
        public IdentifiedByOption GetIdentifiedByOption(string propertyName)
        {
            return IdentifiedByOptions
                .FirstOrDefault(t => t.Property == propertyName)
                ?? new IdentifiedByOption() { IdentifiedByCondition = IdentifiedByCondition.None };
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Entity"/> class by copying from a source entity and associating with a specific individual.
        /// </summary>
        /// <param name="source">The source entity to copy from.</param>
        /// <param name="individual">The ontology individual to associate.</param>
        public Entity(Entity source, Individual individual) : this()
        {
            var ind = individual.Resource as IUriNode;
            Individual = individual;
            Iri = ind.Uri;
            Type = source.Type;
            ScriptVariableName = source.ScriptVariableName;
            OwnerGraph = source.OwnerGraph;
        }

        /// <summary>
        /// Gets all property values for a specified property name.
        /// </summary>
        /// <param name="propertyName">The property name to search for.</param>
        /// <returns>A list of property values.</returns>
        public IList<PropertyValue> GetPropertyValuesByName(string propertyName)
        {
            return Properties.GetPropertyValuesByName(propertyName);
        }

        /// <summary>
        /// Adds a property to the entity.
        /// </summary>
        /// <param name="propertyName">The name of the property.</param>
        /// <param name="propertyType">The type of the property.</param>
        /// <param name="value">The value of the property.</param>
        public void AddProperty(string propertyName, string propertyType, object value)
        {
            Properties.AddProperty(propertyName, new PropertyValue() { PropertyType = propertyType, Value = value, PropertyName = propertyName });
        }

        /// <summary>
        /// Marks a property as an identifier for the entity.
        /// </summary>
        /// <param name="byCondition">The identification condition (e.g., required, optional).</param>
        /// <param name="propertyName">The property name to use as an identifier.</param>
        public void IdentifiedByProperty(IdentifiedByCondition byCondition, string propertyName)
        {
            if (Properties.HasPropertyValue(propertyName))
                IdentifiedByOptions.Add(new IdentifiedByOption() { IdentifiedByCondition = byCondition, IdentifiedByType = IdentifiedByType.Property, Property = propertyName });
        }

        /// <summary>
        /// Marks an entity property as an identifier for the entity.
        /// </summary>
        /// <param name="byCondition">The identification condition.</param>
        /// <param name="propertyName">The property name.</param>
        /// <param name="entity">The entity to use as an identifier.</param>
        public void IdentifiedByEntity(IdentifiedByCondition byCondition, string propertyName, Entity entity)
        {
            if (Properties.HasPropertyValue(propertyName) && entity.IdentifiedByOptions.Count > 0)
                IdentifiedByOptions.Add(new IdentifiedByOption() { IdentifiedByCondition = byCondition, IdentifiedByType = IdentifiedByType.Entity, Property = propertyName, Entity = entity });
        }

        /// <summary>
        /// Determines whether the entity has any properties.
        /// </summary>
        /// <returns>True if the entity has properties; otherwise, false.</returns>
        public bool HasProperties()
        {
            return Properties.HasProperties();
        }
    }
}
