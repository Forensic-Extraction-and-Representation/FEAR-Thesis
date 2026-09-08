using FEAR.Admin.Services.Management;
using FEAR.Blazor.Shared.Components.ConfigurationEditor.Model;
using FEAR.Domain.Model;
using FEAR.Domain.Model.InvestigationStore;
using FEAR.Host.Domain.Api.Investigation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using System.Text.Json;

namespace FEAR.Admin.Pages.Investigation
{
    [Authorize]
    public partial class Queries
    {
        [Inject] InvestigationService InvestigationService { get; set; }

        QueryInvestigations.Response InvestigationResponse { get; set; } = new QueryInvestigations.Response();

        EditorOptions opts { get; set; }
        EditorValueStore ValueStore = null;

        protected override Func<InvestigationInfo, bool> QuickFilters => x=>
        {
            if (string.IsNullOrWhiteSpace(_searchString))
                return true;
            if (x.Name != null && x.Name.Contains(_searchString, StringComparison.OrdinalIgnoreCase))
                return true;
            if (x.Description != null && x.Description.Contains(_searchString, StringComparison.OrdinalIgnoreCase))
                return true;
            if (x.CaseNumber != null && x.CaseNumber.Contains(_searchString, StringComparison.OrdinalIgnoreCase))
                return true;

            return false;
        };

        protected override async Task OnInitializedAsync()
        {
            // Subscribe to the event that triggers the modal display
            LoadInvestigations();
        }

        private async void LoadInvestigations()
        {
            InvestigationResponse = await InvestigationService.QueryInvestigations(new QueryInvestigations.Request());
            Items = InvestigationResponse.Results;
            StateHasChanged();
        }

        private async Task EditInvestigation(InvestigationInfo item)
        {
            var root = await InvestigationService.GetInvestigationConfigurationEditorRoot();
            var configString = JsonSerializer.Serialize(await InvestigationService.GetInvestigationConfiguration(item));
            
            var config = new ConfigurationBuilder()
                .AddJsonStream(new MemoryStream(System.Text.Encoding.UTF8.GetBytes(configString)))
                .Build();

            opts = new EditorOptions()
            {
                Title = "Investigation Configuration Editor",
                Root = root
            };

            ValueStore = new EditorValueStore();
            ValueStore.SetValuesFromConfiguration(config, opts.Root);
            ValueStore.OnValueChanged = StateHasChanged;

            editorDialog.BeginEdit(item);
        }

        private void DeleteInvestigation(InvestigationInfo item)
        {
        }

        private void CreateInvestigation()
        {
            var newApp = new InvestigationInfo();
            EditInvestigation(newApp);
        }

        private async Task SaveInvestigation(InvestigationInfo updatedValue)
        {
            var jsonElement = ValueStore.ToJson(opts.Root);
            var config = jsonElement.Deserialize<CaseConfiguration>();
            // Need to save the configuration as well somehow
            await InvestigationService.SaveInvestigation(updatedValue, config);
        }
    }
}