using FEAR.Admin.Services.Management;
using FEAR.Domain.Model.InvestigationStore;
using FEAR.Host.Domain.Api.Investigation;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using MudBlazor;

namespace FEAR.Admin.Components.Dialogs
{
    public partial class InvestigationInfoDialog
    {
        [CascadingParameter] IMudDialogInstance MudDialog { get; set; }
        [Parameter] public string InvestigationName { get; set; }
        [Parameter] public InvestigationInfo Investigation { get; set; }

        [Inject] InvestigationService InvestigationService { get; set; }
        [Inject] IJSRuntime JSRuntime { get; set; }

        private bool _isLoading = true;

        private GetInvestigationGraphStatistics.Response _stats;
        private bool _isDownloadingGraph;
        private string _graphDownloadError;

        private bool _isLoadingOntology;
        private bool _isOntologyAvailable;
        private string _ontologyContent;
        private string _ontologyError;

        protected override async Task OnInitializedAsync()
        {
            await Task.WhenAll(LoadStatistics(), LoadOntology());
            _isLoading = false;
        }

        private async Task LoadStatistics()
        {
            _graphDownloadError = null;
            try
            {
                _stats = await InvestigationService.GetInvestigationGraphStatistics(Investigation);
            }
            catch (Exception ex)
            {
                _stats = new GetInvestigationGraphStatistics.Response { IsAvailable = false };
                _graphDownloadError = $"Failed to load graph statistics: {ex.Message}";
            }
            StateHasChanged();
        }

        private async Task LoadOntology()
        {
            _isLoadingOntology = true;
            _ontologyError = null;
            try
            {
                var response = await InvestigationService.GetInvestigationOntology(Investigation);
                _isOntologyAvailable = response?.IsAvailable ?? false;
                _ontologyContent = response?.OntologyContent;
            }
            catch (Exception ex)
            {
                _isOntologyAvailable = false;
                _ontologyError = $"Failed to load ontology: {ex.Message}";
            }
            finally
            {
                _isLoadingOntology = false;
            }
            StateHasChanged();
        }

        private async Task DownloadGraphData()
        {
            _isDownloadingGraph = true;
            _graphDownloadError = null;
            StateHasChanged();
            try
            {
                var response = await InvestigationService.GetInvestigationGraphData(Investigation);
                if (response?.IsAvailable == true)
                {
                    var filename = $"{SanitizeFilename(InvestigationName)}-graph-{DateTime.UtcNow:yyyyMMddHHmmss}.ttl";
                    await JSRuntime.InvokeVoidAsync("fearDownloadJson", filename, response.TurtleContent);
                }
                else
                {
                    _graphDownloadError = "Graph data is not available. Ensure the investigation graph service is running and has processed artifacts.";
                }
            }
            catch (Exception ex)
            {
                _graphDownloadError = $"Failed to download graph data: {ex.Message}";
            }
            finally
            {
                _isDownloadingGraph = false;
                StateHasChanged();
            }
        }

        private async Task DownloadOntology()
        {
            if (!_isOntologyAvailable || string.IsNullOrWhiteSpace(_ontologyContent))
                return;

            var filename = $"{SanitizeFilename(InvestigationName)}-ontology-{DateTime.UtcNow:yyyyMMddHHmmss}.ttl";
            await JSRuntime.InvokeVoidAsync("fearDownloadJson", filename, _ontologyContent);
        }

        private void Close() => MudDialog.Close();

        private static string SanitizeFilename(string name) =>
            string.Concat(name.Select(c => Path.GetInvalidFileNameChars().Contains(c) ? '_' : c));
    }
}
