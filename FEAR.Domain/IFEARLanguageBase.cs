namespace FEAR.Domain
{
    /// <summary>
    /// Provides a base contract for FEAR language components 
    /// that require ontology and namespace management.
    /// Implementations support mapping between prefixes and 
    /// URIs for ontology imports and namespace usage in the 
    /// knowledge graph.
    /// </summary>
    public interface IFEARLanguageBase
    {
        /// <summary>
        /// Gets the collection of ontology namespace imports, 
        /// mapping a prefix to its corresponding ontology URI.
        /// Used to declare and manage external ontologies 
        /// referenced in the knowledge graph.
        /// </summary>
        Dictionary<string, Uri> OntologyNamespaceImports { get; }

        /// <summary>
        /// Gets the collection of namespace prefixes, mapping a 
        /// prefix to its corresponding namespace URI.
        /// Used for compact representation and resolution of 
        /// URIs in graph queries and codification.
        /// </summary>
        Dictionary<string, Uri> NamespacePrefixes { get; }
    }
}
