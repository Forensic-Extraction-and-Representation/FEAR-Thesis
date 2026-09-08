using AutoMapper.Internal;
using FEAR.Domain.KnowledgeGraph.Graphs;
using VDS.RDF;

namespace FEAR.Domain.KnowledgeGraph
{
    /// <summary>
    /// Builder class for constructing a set of <see cref="LiteralDefinition"/> objects
    /// and converting them into RDF literal nodes for use in a knowledge graph.
    /// Supports fluent addition of literals and batch creation of literal nodes.
    /// </summary>
    public class LiteralDefinitionBuilder
    {
        /// <summary>
        /// Gets or sets whether to ignore null values when adding literals.
        /// If false, an exception is thrown when a null value is added.
        /// </summary>
        private bool IgnoreNullValues { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="LiteralDefinitionBuilder"/> class.
        /// </summary>
        /// <param name="ignoreNullValues">If true, null values are ignored; otherwise, an exception is thrown for null values.</param>
        public LiteralDefinitionBuilder(bool ignoreNullValues = true)
        {
            IgnoreNullValues = ignoreNullValues;
        }

        // Internal list of literal definitions to be built.
        List<LiteralDefinition> _definitions = new List<LiteralDefinition>();

        /// <summary>
        /// Adds a literal definition using a property, value, and .NET type.
        /// Handles special formatting for DateTime values.
        /// </summary>
        /// <param name="property">The property URI or name.</param>
        /// <param name="value">The value to add.</param>
        /// <param name="objectType">The .NET type of the value.</param>
        /// <returns>The builder instance for chaining.</returns>
        /// <exception cref="ArgumentNullException">Thrown if value is null and <see cref="IgnoreNullValues"/> is false.</exception>
        public LiteralDefinitionBuilder WithLiteral(string property, object value, Type objectType)
        {
            if (value != null)
                if (objectType == typeof(DateTime) || objectType == typeof(DateTime?))
                {
                    // Handle DateTime values specifically, formatting them as ISO 8601 strings
                    if (!objectType.IsNullableType() || ((DateTime?)value).HasValue)
                        _definitions.Add(new LiteralDefinition(property, ((DateTime)value).ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss.fff'Z'"), objectType));
                }
                else
                    _definitions.Add(new LiteralDefinition(property, value, objectType));
            else
                if (!IgnoreNullValues)
                    throw new ArgumentNullException($"Value for {property} was null");

            return this;
        }

        /// <summary>
        /// Adds a literal definition using a property, value, and XSD type URI.
        /// </summary>
        /// <param name="property">The property URI or name.</param>
        /// <param name="value">The value to add.</param>
        /// <param name="objectType">The XSD type URI of the value.</param>
        /// <returns>The builder instance for chaining.</returns>
        public LiteralDefinitionBuilder WithLiteral(string property, object value, string objectType)
        {
            _definitions.Add(new LiteralDefinition(property, value, objectType));
            return this;
        }

        /// <summary>
        /// Adds a literal definition using a property and value, inferring the type from the value.
        /// </summary>
        /// <typeparam name="T">The type of the value.</typeparam>
        /// <param name="property">The property URI or name.</param>
        /// <param name="value">The value to add.</param>
        /// <returns>The builder instance for chaining.</returns>
        public LiteralDefinitionBuilder WithLiteral<T>(string property, T value)
        {
            return WithLiteral(property, value, typeof(T));
        }

        /// <summary>
        /// Creates a new builder and adds a literal definition using a property, value, and XSD type URI.
        /// </summary>
        /// <param name="property">The property URI or name.</param>
        /// <param name="value">The value to add.</param>
        /// <param name="objectType">The XSD type URI of the value.</param>
        /// <returns>A new builder instance with the literal added.</returns>
        public static LiteralDefinitionBuilder CreateWithLiteral(string property, object value, string objectType)
        {
            return new LiteralDefinitionBuilder().WithLiteral(property, value, objectType);
        }

        /// <summary>
        /// Creates a new builder and adds a literal definition using a property, value, and .NET type.
        /// </summary>
        /// <param name="property">The property URI or name.</param>
        /// <param name="value">The value to add.</param>
        /// <param name="objectType">The .NET type of the value.</param>
        /// <returns>A new builder instance with the literal added.</returns>
        public static LiteralDefinitionBuilder CreateWithLiteral(string property, object value, Type objectType)
        {
            return new LiteralDefinitionBuilder().WithLiteral(property, value, objectType);
        }

        /// <summary>
        /// Creates a new builder and adds a literal definition using a property and value, inferring the type from the value.
        /// </summary>
        /// <typeparam name="T">The type of the value.</typeparam>
        /// <param name="property">The property URI or name.</param>
        /// <param name="value">The value to add.</param>
        /// <returns>A new builder instance with the literal added.</returns>
        public static LiteralDefinitionBuilder CreateWithLiteral<T>(string property, T value)
        {
            return new LiteralDefinitionBuilder().WithLiteral(property, value);
        }

        /// <summary>
        /// Creates a new builder instance with the specified null value handling behavior.
        /// </summary>
        /// <param name="ignoreNullValue">If true, null values are ignored; otherwise, an exception is thrown for null values.</param>
        /// <returns>A new builder instance.</returns>
        public static LiteralDefinitionBuilder Create(bool ignoreNullValue = true)
        {
            return new LiteralDefinitionBuilder(ignoreNullValue);
        }

        /// <summary>
        /// Builds the collection of literal definitions into a dictionary of RDF literal nodes, keyed by property URI.
        /// Uses the provided <see cref="IRealGraph"/> to create or retrieve the literal nodes.
        /// </summary>
        /// <param name="forGraph">The graph in which to create the literal nodes.</param>
        /// <returns>A dictionary mapping property URIs to RDF literal nodes.</returns>
        public Dictionary<Uri, ILiteralNode> Build(IRealGraph forGraph)
        {
            Dictionary<Uri, ILiteralNode> nodes = new Dictionary<Uri, ILiteralNode>();
            foreach (var l in _definitions)
            {
                ILiteralNode ln = forGraph.Graph.GetLiteralNode(l.PropertyValue) ?? forGraph.Graph.CreateLiteralNode(l.PropertyValue, forGraph.GraphManager.GetUriFromPrefixNotation(l.TypeUri).Value);
                nodes.Add(forGraph.GraphManager.GetUriFromPrefixNotation(l.PropertyUri).Value, ln);
            }
            return nodes;
        }
    }
}