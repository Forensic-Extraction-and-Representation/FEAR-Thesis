namespace FEAR.Host.Domain.Api.Investigation
{
    /// <summary>
    /// Tracks which sections and agent secrets were included in a case export.
    /// Embedded in the export package and used during import to know what data is present
    /// and which secrets need to be collected from the user.
    /// </summary>
    public class CaseExportManifest
    {
        /// <summary>
        /// Optional message shown to the importing user before the import begins.
        /// </summary>
        public string? ImportMessage { get; set; }

        /// <summary>
        /// The sections of the case configuration that were included in the export.
        /// Valid values: "Directories", "GraphSettings", "ConnectionSettings", "Repositories", "AgentConfiguration", "Queries".
        /// </summary>
        public List<string> IncludedSections { get; set; } = new List<string>();

        /// <summary>
        /// Records the export decision for each agent secret field.
        /// Secrets with <see cref="AgentSecretEntry.IsIncluded"/> = false must be supplied during import.
        /// </summary>
        public List<AgentSecretEntry> AgentSecrets { get; set; } = new List<AgentSecretEntry>();
    }

    /// <summary>
    /// Represents a single agent secret field and whether it was included in the export.
    /// </summary>
    public class AgentSecretEntry
    {
        /// <summary>The agent's key as it appears in <see cref="CaseConfiguration.AgentOptions.Agents"/>.</summary>
        public string AgentKey { get; set; } = string.Empty;

        /// <summary>The option field name that holds the secret value.</summary>
        public string FieldName { get; set; } = string.Empty;

        /// <summary>Whether the secret value was written into the export package.</summary>
        public bool IsIncluded { get; set; }
    }
}
