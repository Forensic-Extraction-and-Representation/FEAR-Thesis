using FEAR.Domain;

namespace FEAR.Runtime.Compiler
{
    /// <summary>
    /// Provides a base implementation for FEAR language components that require ontology and namespace management.
    /// Supports mapping between prefixes and URIs for ontology imports and namespace usage in the knowledge graph.
    /// Implements logic for resolving RDF types, adding namespace prefixes, and handling default namespaces.
    /// </summary>
    public abstract class FEARLanguageBase : IFEARLanguageBase
    {
        /// <summary>
        /// Resolves a type string to a URI, supporting absolute URIs, prefixed names, and default namespaces.
        /// </summary>
        /// <param name="type">The type string to resolve (absolute URI, prefixed name, or local name).</param>
        /// <returns>The resolved <see cref="Uri"/>.</returns>
        /// <exception cref="Exception">Thrown if the prefix is invalid or cannot be resolved.</exception>
        public Uri ResolveType(string type)
        {
            if (type.StartsWith("http://") || type.StartsWith("https://"))
            {
                return new Uri(type);
            }
            else if (type.Contains(":"))
            {
                // Handles prefixed names like "rdf:Type" or "owl:Class"
                var split = type.Split(':');
                if (NamespacePrefixes.ContainsKey(split[0]))
                {
                    return new Uri(NamespacePrefixes[split[0]], split[1]);
                }
                else if (OntologyNamespaceImports.ContainsKey(split[0]))
                {
                    return new Uri(OntologyNamespaceImports[split[0]], split[1]);
                }
                else
                {
                    throw new Exception("Invalid prefix");
                }
            }
            else
            {
                return new Uri(OntologyNamespaceImports.First().Value, type);
            }
        }

        /// <summary>
        /// Adds a default namespace prefix using a well-known RDF/OWL/XSD/XML prefix.
        /// </summary>
        /// <param name="prefix">The prefix to add (e.g., "rdf", "owl").</param>
        public void AddDefaultNamespace(string prefix)
        {
            NamespacePrefixes.Add(prefix, new Uri(ResolveDefaultNamespace(prefix)));
        }

        /// <summary>
        /// Resolves a well-known prefix to its default namespace URI.
        /// </summary>
        /// <param name="prefix">The prefix to resolve.</param>
        /// <returns>The default namespace URI as a string.</returns>
        /// <exception cref="Exception">Thrown if the prefix is not recognized.</exception>
        public static string ResolveDefaultNamespace(string prefix)
        {
            switch (prefix)
            {
                case "rdf":
                    return "http://www.w3.org/1999/02/22-rdf-syntax-ns#";
                case "rdfs":
                    return "http://www.w3.org/2000/01/rdf-schema#";
                case "owl":
                    return "http://www.w3.org/2002/07/owl#";
                case "xsd":
                    return "http://www.w3.org/2001/XMLSchema#";
                case "xml":
                    return "http://www.w3.org/XML/1998/namespace";
                case "swrlb":
                    return "http://www.w3.org/2003/11/swrlb#";
                default:
                    throw new Exception("Invalid prefix");
            }
        }

        /// <summary>
        /// Adds a custom namespace prefix and its associated absolute URI.
        /// </summary>
        /// <param name="prefix">The prefix to add.</param>
        /// <param name="uri">The absolute URI for the namespace.</param>
        public void AddNamespacePrefix(string prefix, Uri uri)
        {
            if (uri.IsAbsoluteUri)
                NamespacePrefixes.Add(prefix, uri);
        }

        /// <summary>
        /// Gets the collection of ontology namespace imports, mapping a prefix to its corresponding ontology URI.
        /// Used to declare and manage external ontologies referenced in the knowledge graph.
        /// </summary>
        public abstract Dictionary<string, Uri> OntologyNamespaceImports { get; }

        /// <summary>
        /// Gets or sets the collection of namespace prefixes, mapping a prefix to its corresponding namespace URI.
        /// Used for compact representation and resolution of URIs in graph queries and codification.
        /// </summary>
        public Dictionary<string, Uri> NamespacePrefixes { get; set; } = new Dictionary<string, Uri>();
    }
}
