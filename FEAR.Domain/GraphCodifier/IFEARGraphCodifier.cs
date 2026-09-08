namespace FEAR.Domain.GraphCodifier
{
    /// <summary>
    /// Defines a contract for FEAR graph codifiers, which are responsible 
    /// for mapping or codifying domain objects into a knowledge graph.
    /// Codifiers can be discovered and executed dynamically, and provide 
    /// metadata about their category, name, and accepted properties.
    /// </summary>
    public interface IFEARGraphCodifier : IFEARLanguageBase
    {
        /// <summary>
        /// Executes the codification logic using the provided graph codifier context.
        /// This method is called to perform the actual mapping or transformation of data into the knowledge graph.
        /// </summary>
        /// <param name="context">The context containing data and services for codification.</param>
        void Execute(IGraphCodifierContext context);

        /// <summary>
        /// Gets the category of the graph codifier, typically representing a logical grouping or namespace.
        /// </summary>
        string GraphCodifierCategory { get; }

        /// <summary>
        /// Gets the name of the codifier, used for identification and discovery.
        /// </summary>
        string CodifierName { get; }

        /// <summary>
        /// Gets the list of property names that this codifier accepts as input for codification.
        /// </summary>
        string[] AcceptsProperties { get; }
    }
}
