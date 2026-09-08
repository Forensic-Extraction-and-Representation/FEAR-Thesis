using FEAR.Admin.Services.Management;
using FEAR.Domain.Model;
using FEAR.Domain.Model.InvestigationStore;
using FEAR.Host.Domain.Api.Investigation;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using MudBlazor;
using System.Text.Json;

namespace FEAR.Admin.Components.Case
{
    public partial class ExportCaseDialog
    {
        [CascadingParameter] IMudDialogInstance MudDialog { get; set; } = default!;

        [Parameter] public InvestigationInfo Investigation { get; set; } = default!;
        [Parameter] public CaseConfiguration Configuration { get; set; } = default!;

        /// <summary>Agent adapter field metadata loaded from the API, used to identify secret fields.</summary>
        [Parameter] public List<ConfigurationFieldOption> AgentAdapterTypes { get; set; } = new();

        [Inject] InvestigationService InvestigationService { get; set; } = default!;
        [Inject] IJSRuntime JSRuntime { get; set; } = default!;

        private string? _importMessage;
        private bool _includeDirectories = true;
        private bool _includeGraphSettings = true;
        private bool _includeConnectionSettings = true;
        private bool _includeRepositories = true;
        private bool _includeAgentConfiguration = true;
        private bool _includeQueries = true;

        private List<ExportableAgentSecret> _agentSecrets = new();

        private static readonly HashSet<string> _secretNameHints = new(StringComparer.OrdinalIgnoreCase)
        {
            "apikey", "key", "secret", "password", "token", "credential", "credentials"
        };

        protected override void OnParametersSet()
        {
            BuildAgentSecretsList();
        }

        private void BuildAgentSecretsList()
        {
            _agentSecrets.Clear();

            if (Configuration?.AgentConfiguration?.Agents == null)
                return;

            foreach (var (agentKey, agentDef) in Configuration.AgentConfiguration.Agents)
            {
                // Find registered field metadata for this agent type (if any)
                var adapterMeta = AgentAdapterTypes
                    .FirstOrDefault(a => a.Name == agentDef.AgentType);

                foreach (var (optionName, _) in agentDef.AgentOptions)
                {
                    bool isSecret = false;

                    // Check registered metadata first
                    var fieldMeta = adapterMeta?.FieldMetadata?
                        .FirstOrDefault(m => string.Equals(m.Name, optionName, StringComparison.OrdinalIgnoreCase));

                    if (fieldMeta != null)
                    {
                        isSecret = fieldMeta.IsSecret;
                    }
                    else
                    {
                        // Fallback: heuristic name match
                        isSecret = _secretNameHints.Contains(optionName)
                            || _secretNameHints.Any(h => optionName.Contains(h, StringComparison.OrdinalIgnoreCase));
                    }

                    if (isSecret)
                    {
                        _agentSecrets.Add(new ExportableAgentSecret
                        {
                            AgentKey = agentKey,
                            FieldName = optionName,
                            IsIncluded = false  // default: do not export secrets
                        });
                    }
                }
            }
        }

        private List<string> BuildIncludedSections()
        {
            var sections = new List<string>();
            if (_includeDirectories) sections.Add("Directories");
            if (_includeGraphSettings) sections.Add("GraphSettings");
            if (_includeConnectionSettings) sections.Add("ConnectionSettings");
            if (_includeRepositories) sections.Add("Repositories");
            if (_includeAgentConfiguration) sections.Add("AgentConfiguration");
            if (_includeQueries) sections.Add("Queries");
            return sections;
        }

        private async Task Export()
        {
            var request = new ExportCase.Request
            {
                InvestigationId = Investigation.InvestigationId,
                IncludedSections = BuildIncludedSections(),
                AgentSecrets = _includeAgentConfiguration
                    ? _agentSecrets.Select(s => new AgentSecretEntry
                    {
                        AgentKey = s.AgentKey,
                        FieldName = s.FieldName,
                        IsIncluded = s.IsIncluded
                    }).ToList()
                    : new List<AgentSecretEntry>(),
                ImportMessage = string.IsNullOrWhiteSpace(_importMessage) ? null : _importMessage.Trim()
            };

            var response = await InvestigationService.ExportCase(request);

            var json = JsonSerializer.Serialize(response.Package, new JsonSerializerOptions { WriteIndented = true });
            var filename = $"case-export-{SanitizeFilename(Investigation.Name)}-{DateTime.UtcNow:yyyyMMddHHmmss}.json";

            await JSRuntime.InvokeVoidAsync("fearDownloadJson", filename, json);

            MudDialog.Close(DialogResult.Ok(true));
        }

        private void Cancel() => MudDialog.Cancel();

        private static string SanitizeFilename(string name) =>
            string.Concat(name.Select(c => Path.GetInvalidFileNameChars().Contains(c) ? '_' : c));

        /// <summary>Binding model for the agent secrets table.</summary>
        private class ExportableAgentSecret
        {
            public string AgentKey { get; set; } = string.Empty;
            public string FieldName { get; set; } = string.Empty;
            public bool IsIncluded { get; set; }
        }
    }
}
