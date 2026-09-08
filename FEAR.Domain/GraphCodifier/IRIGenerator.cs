using FEAR.Domain.KnowledgeGraph.IRI;

namespace FEAR.Domain.GraphCodifier
{
    /// <summary>
    /// Generates IRIs (Internationalized Resource Identifiers) for entities in the knowledge graph
    /// using a specified investigation namespace abbreviation. Inherits core IRI generation logic
    /// from <see cref="BaseIRIGenerator"/> and is typically used to ensure consistent, case-specific
    /// IRI creation for graph codification and entity management.
    /// </summary>
    public class IRIGenerator : BaseIRIGenerator
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="IRIGenerator"/> class with the given investigation namespace abbreviation.
        /// </summary>
        /// <param name="investigationNamespaceAbbrev">The namespace abbreviation to use for IRI generation.</param>
        public IRIGenerator(string investigationNamespaceAbbrev) : base(investigationNamespaceAbbrev)
        {
        }
    }
}