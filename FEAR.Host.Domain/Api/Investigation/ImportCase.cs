using FEAR.Domain.Model;
using FEAR.Domain.Model.InvestigationStore;

namespace FEAR.Host.Domain.Api.Investigation
{
    public class ImportCase
    {
        /// <summary>
        /// Request sent from the admin UI to import a case package.
        /// Secrets that were omitted from the export are supplied here.
        /// </summary>
        public class Request
        {
            /// <summary>The export package to import.</summary>
            public CaseExportPackage Package { get; set; } = new CaseExportPackage();

            /// <summary>
            /// Secret values collected from the user during the import wizard.
            /// Key: "{AgentKey}:{FieldName}", Value: the secret string.
            /// </summary>
            public Dictionary<string, string> SuppliedSecrets { get; set; } = new Dictionary<string, string>();
        }

        /// <summary>Response returned after a successful import.</summary>
        public class Response
        {
            public bool Success { get; set; }
            public Guid InvestigationId { get; set; }
            public string? Message { get; set; }
        }
    }
}
