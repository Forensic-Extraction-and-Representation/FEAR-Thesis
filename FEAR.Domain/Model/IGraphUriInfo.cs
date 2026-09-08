namespace FEAR.Domain.Model
{
    /// <summary>
    /// Provides properties for managing RDF/graph namespace information.
    /// Implementing types can be used to associate entities with a specific graph namespace and its abbreviation.
    /// </summary>
    public interface IGraphUriInfo
    {
        /// <summary>
        /// Gets or sets the RDF/graph namespace URI associated with the entity.
        /// </summary>
        string Namespace { get; set; }

        /// <summary>
        /// Gets or sets the abbreviated form of the namespace, useful for compact queries or display.
        /// </summary>
        string NamespaceAbbrev { get; set; }
    }
}
