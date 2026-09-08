namespace FEAR.Domain.Model.InvestigationStore
{
    /// <summary>
    /// Represents summary information about an investigation, including case details and graph namespace information.
    /// Inherits the investigation identifier from <see cref="InvestigationIdObject"/> and graph URI info from <see cref="IGraphUriInfo"/>.
    /// </summary>
    public class InvestigationInfo : InvestigationIdObject, IGraphUriInfo
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="InvestigationInfo"/> class.
        /// </summary>
        public InvestigationInfo()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="InvestigationInfo"/> class by copying values from another instance.
        /// </summary>
        /// <param name="inv">The <see cref="InvestigationInfo"/> instance to copy.</param>
        public InvestigationInfo(InvestigationInfo inv)
        {
            InvestigationId = inv.InvestigationId;
            Name = inv.Name;
            Description = inv.Description;
            CaseDate = inv.CaseDate;
            CaseNumber = inv.CaseNumber;
            CaseType = inv.CaseType;
            CaseStatus = inv.CaseStatus;
            Namespace = inv.Namespace;
            NamespaceAbbrev = inv.NamespaceAbbrev;
        }

        /// <summary>
        /// Gets or sets the name of the investigation.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the description of the investigation.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the case number associated with the investigation.
        /// </summary>
        public string CaseNumber { get; set; } = String.Empty;

        /// <summary>
        /// Gets or sets the arbitrary type of the case.
        /// </summary>
        public string CaseType { get; set; } = String.Empty;

        /// <summary>
        /// Gets or sets the current status of the case (e.g., open, closed, pending).
        /// </summary>
        public string CaseStatus { get; set; } = String.Empty;

        /// <summary>
        /// Gets or sets the date associated with the case (e.g., date opened or occurred).
        /// </summary>
        public DateTime CaseDate { get; set; }

        /// <summary>
        /// Gets or sets the RDF/graph namespace for this investigation.
        /// Used for knowledge graph or linked data integration.
        /// </summary>
        public string Namespace { get; set; } = String.Empty;

        /// <summary>
        /// Gets or sets the abbreviated form of the namespace for this investigation.
        /// Useful for compact graph queries or display.
        /// </summary>
        public string NamespaceAbbrev { get; set; } = String.Empty;
    }
}
