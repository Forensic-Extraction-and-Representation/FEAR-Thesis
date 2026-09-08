namespace FEAR.Domain.Model
{
    /// <summary>
    /// Concrete implementation of <see cref="IGraphUriInfo"/> for storing graph namespace and abbreviation information.
    /// </summary>
    public class GraphUriInfo : IGraphUriInfo
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GraphUriInfo"/> class.
        /// </summary>
        public GraphUriInfo() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="GraphUriInfo"/> class with the specified namespace and abbreviation.
        /// </summary>
        /// <param name="ns">The RDF/graph namespace URI.</param>
        /// <param name="nsAbbrev">The abbreviated form of the namespace.</param>
        public GraphUriInfo(string ns, string nsAbbrev)
        {
            Namespace = ns;
            NamespaceAbbrev = nsAbbrev;
        }

        /// <summary>
        /// Gets or sets the RDF/graph namespace URI associated with the entity.
        /// </summary>
        public string Namespace { get; set; }

        /// <summary>
        /// Gets or sets the abbreviated form of the namespace.
        /// </summary>
        public string NamespaceAbbrev { get; set; }
    }
}
