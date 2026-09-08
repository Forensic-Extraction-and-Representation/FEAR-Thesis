using FEAR.Admin.Services.Management;
using FEAR.Domain.Model.InvestigationStore;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace FEAR.Admin.Components.Dialogs
{
    public partial class ViewOntologyDialog
    {
        [CascadingParameter] IMudDialogInstance MudDialog { get; set; }
        [Parameter] public string InvestigationName { get; set; }
        [Parameter] public InvestigationInfo Investigation { get; set; }

        [Inject] InvestigationService InvestigationService { get; set; }

        private bool _isLoading = true;
        private bool _isAvailable;
        private bool _hasError;
        private string _errorMessage = string.Empty;
        private string _ontologyContent;

        protected override async Task OnInitializedAsync()
        {
            await LoadOntology();
        }

        private async Task LoadOntology()
        {
            _isLoading = true;
            _hasError = false;
            try
            {
                var response = await InvestigationService.GetInvestigationOntology(Investigation);
                _isAvailable = response?.IsAvailable ?? false;
                _ontologyContent = response?.OntologyContent;
            }
            catch (Exception ex)
            {
                _hasError = true;
                _errorMessage = $"Failed to load ontology: {ex.Message}";
                _isAvailable = false;
            }
            finally
            {
                _isLoading = false;
            }
        }

        private void Close() => MudDialog.Close();
    }
}
