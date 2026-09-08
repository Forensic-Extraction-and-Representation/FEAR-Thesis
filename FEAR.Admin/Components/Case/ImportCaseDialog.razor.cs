using FEAR.Admin.Services.Management;
using FEAR.Host.Domain.Api.Investigation;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.JSInterop;
using MudBlazor;
using System.Text.Json;

namespace FEAR.Admin.Components.Case
{
    public partial class ImportCaseDialog
    {
        [CascadingParameter] IMudDialogInstance MudDialog { get; set; } = default!;

        [Inject] InvestigationService InvestigationService { get; set; } = default!;
        [Inject] IJSRuntime JSRuntime { get; set; } = default!;

        private enum ImportStep
        {
            SelectFile,
            ShowMessage,
            ReviewSections,
            SupplySecrets,
            Confirm,
            Done
        }

        private ImportStep _step = ImportStep.SelectFile;
        private CaseExportPackage? _package;
        private string? _selectedFileName;
        private string? _fileError;
        private bool _isBusy;
        private string? _resultMessage;

        private List<MissingSecretEntry> _missingSecrets = new();

        private static readonly Dictionary<string, string> _allSections = new()
        {
            { "Directories",        "Directories" },
            { "GraphSettings",      "Graph Settings" },
            { "ConnectionSettings", "Connection Settings" },
            { "Repositories",       "Repositories" },
            { "AgentConfiguration", "Agent Configuration" },
            { "Queries",            "Queries" }
        };

        private bool SecretsComplete =>
            _missingSecrets.All(s => !string.IsNullOrWhiteSpace(s.Value));

        private async Task OnFileSelected(InputFileChangeEventArgs e)
        {
            _fileError = null;
            _package = null;
            _selectedFileName = null;

            var file = e.File;
            if (file == null)
                return;

            _selectedFileName = file.Name;

            try
            {
                using var stream = file.OpenReadStream(maxAllowedSize: 10 * 1024 * 1024);
                _package = await JsonSerializer.DeserializeAsync<CaseExportPackage>(stream,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (_package == null)
                    _fileError = "The selected file could not be parsed as a valid case export.";
            }
            catch (Exception ex)
            {
                _fileError = $"Failed to read file: {ex.Message}";
                _package = null;
            }

            StateHasChanged();
        }

        private async Task _clickFileInput()
        {
            await JSRuntime.InvokeVoidAsync("eval",
                "document.getElementById('importFileInput').click()");
        }

        private void NextFromSelectFile()
        {
            if (_package == null) return;

            _missingSecrets = _package.Manifest.AgentSecrets
                .Where(s => !s.IsIncluded)
                .Select(s => new MissingSecretEntry { AgentKey = s.AgentKey, FieldName = s.FieldName })
                .ToList();

            // Advance past "ShowMessage" if there is no message
            if (!string.IsNullOrWhiteSpace(_package.Manifest.ImportMessage))
                _step = ImportStep.ShowMessage;
            else
                _step = ImportStep.ReviewSections;
        }

        private void GoToNextStep()
        {
            _step = _step switch
            {
                ImportStep.ShowMessage    => ImportStep.ReviewSections,
                ImportStep.ReviewSections => _missingSecrets.Count > 0 ? ImportStep.SupplySecrets : ImportStep.Confirm,
                ImportStep.SupplySecrets  => ImportStep.Confirm,
                _                         => _step
            };
        }

        private async Task DoImport()
        {
            if (_package == null) return;
            _isBusy = true;

            try
            {
                var suppliedSecrets = _missingSecrets.ToDictionary(
                    s => $"{s.AgentKey}:{s.FieldName}",
                    s => s.Value ?? string.Empty);

                var response = await InvestigationService.ImportCase(new ImportCase.Request
                {
                    Package = _package,
                    SuppliedSecrets = suppliedSecrets
                });

                _resultMessage = response.Message ?? "Import completed.";
                _step = ImportStep.Done;
            }
            catch (Exception ex)
            {
                _resultMessage = $"Import failed: {ex.Message}";
                _step = ImportStep.Done;
            }
            finally
            {
                _isBusy = false;
            }
        }

        private void Cancel()
        {
            if (_step == ImportStep.Done)
                MudDialog.Close(DialogResult.Ok(_step == ImportStep.Done));
            else
                MudDialog.Cancel();
        }

        private class MissingSecretEntry
        {
            public string AgentKey { get; set; } = string.Empty;
            public string FieldName { get; set; } = string.Empty;
            public string? Value { get; set; }
        }
    }
}
