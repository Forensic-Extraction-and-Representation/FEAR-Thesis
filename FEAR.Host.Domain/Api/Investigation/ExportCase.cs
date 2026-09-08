using FEAR.Domain.Model;
using FEAR.Domain.Model.InvestigationStore;

namespace FEAR.Host.Domain.Api.Investigation
{
    /// <summary>
    /// The self-contained JSON package written to disk during a case export.
    /// </summary>
    public class CaseExportPackage
    {
        /// <summary>Manifest describing what is included and which secrets are absent.</summary>
        public CaseExportManifest Manifest { get; set; } = new CaseExportManifest();

        /// <summary>Case metadata without the database ID or user assignments.</summary>
        public CaseExportInfo CaseInfo { get; set; } = new CaseExportInfo();

        /// <summary>The case configuration, potentially with secret values redacted.</summary>
        public CaseConfiguration? Configuration { get; set; }

        /// <summary>Saved queries attached to the case. Null or empty when the Queries section was not exported.</summary>
        public List<CaseExportQuery>? Queries { get; set; }
    }

    /// <summary>
    /// A saved query stripped of database identifiers, suitable for export and re-import.
    /// </summary>
    public class CaseExportQuery
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string QueryText { get; set; } = string.Empty;
    }

    /// <summary>
    /// Case metadata stripped of database identifiers and user assignments, suitable for export.
    /// </summary>
    public class CaseExportInfo
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string CaseNumber { get; set; } = string.Empty;
        public string CaseType { get; set; } = string.Empty;
        public string CaseStatus { get; set; } = string.Empty;
        public DateTime CaseDate { get; set; }
        public string Namespace { get; set; } = string.Empty;
        public string NamespaceAbbrev { get; set; } = string.Empty;
    }

    public class ExportCase
    {
        /// <summary>Request sent from the admin UI to produce an export package.</summary>
        public class Request
        {
            public Guid InvestigationId { get; set; }

            /// <summary>Sections to include. Omitted sections are stripped from the configuration.</summary>
            public List<string> IncludedSections { get; set; } = new List<string>();

            /// <summary>
            /// Per-agent secrets to include. Entries with <see cref="AgentSecretEntry.IsIncluded"/> = false
            /// will have their value redacted from the export.
            /// </summary>
            public List<AgentSecretEntry> AgentSecrets { get; set; } = new List<AgentSecretEntry>();

            /// <summary>Optional message shown to the user when they import this package.</summary>
            public string? ImportMessage { get; set; }
        }

        /// <summary>The serialised export package returned to the client for download.</summary>
        public class Response
        {
            public CaseExportPackage Package { get; set; } = new CaseExportPackage();
        }
    }
}
