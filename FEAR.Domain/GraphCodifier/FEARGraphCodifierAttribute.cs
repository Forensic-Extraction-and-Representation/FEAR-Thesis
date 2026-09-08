namespace FEAR.Domain.GraphCodifier
{
    /// <summary>
    /// Attribute used to mark a class as a FEAR graph codifier.
    /// Codifiers are used to define how domain objects are mapped 
    /// or codified into the knowledge graph. This attribute provides 
    /// metadata for categorizing and identifying codifiers, 
    /// supporting discovery and registration.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class FEARGraphCodifierAttribute : Attribute
    {
        /// <summary>
        /// Gets or sets the name of the codifier.
        /// </summary>
        public string CodifierName { get; set; }

        /// <summary>
        /// Gets or sets the category of the codifier, typically representing a logical grouping or namespace.
        /// </summary>
        public string CodifierCategory { get; set; }

        /// <summary>
        /// Gets the full name of the codifier, combining category and name.
        /// </summary>
        public string FullName => $"{CodifierCategory.TrimEnd('/')}/{CodifierName}";

        /// <summary>
        /// Initializes a new instance of the <see cref="FEARGraphCodifierAttribute"/> class with the specified category and name.
        /// </summary>
        /// <param name="category">The codifier category (e.g., logical grouping or namespace).</param>
        /// <param name="name">The codifier name.</param>
        public FEARGraphCodifierAttribute(string category, string name)
        {
            CodifierName = name;
            CodifierCategory = category;
        }
    }
}
