using FEAR.Domain.GraphCodifier;
using FEAR.Runtime.Compiler;

namespace FEAR.GFEAR
{
    /// <summary>
    /// Provides a base class for FEAR graph codifiers that process data for knowledge graph codification.
    ///
    /// Implementations of this class are responsible for transforming and mapping data into the knowledge graph.
    /// The data to be processed is always sourced from a queue and can be either:
    ///   - An artifact directly (raw evidence or input data)
    ///   - The result of a collector (CFEAR) script
    ///
    /// Derived codifiers implement the <see cref="Execute"/> method to perform the actual codification logic,
    /// using the provided <see cref="IGraphCodifierContext"/> to access the queued data and interact with the graph.
    ///
    /// This base class also exposes metadata about the codifier, including its category, name, and accepted properties.
    /// </summary>
    public abstract class GraphCodifierBase : FEARLanguageBase, IFEARGraphCodifier
    {
        /// <summary>
        /// Executes the codification logic using the provided graph codifier context.
        /// The context provides access to the queued data (artifact or collector result) and graph services.
        /// </summary>
        /// <param name="context">The context containing data and services for codification.</param>
        public abstract void Execute(IGraphCodifierContext context);

        /// <summary>
        /// Gets the category of the graph codifier, typically representing a logical grouping or namespace.
        /// </summary>
        public abstract string GraphCodifierCategory { get; }

        /// <summary>
        /// Gets the name of the codifier, used for identification and discovery.
        /// </summary>
        public abstract string CodifierName { get; }

        /// <summary>
        /// Gets the list of property names that this codifier accepts as input for codification.
        /// </summary>
        public abstract string[] AcceptsProperties { get; }
    }
}
