using FEAR.Admin.Components.Dialogs;
using FEAR.Admin.Services.Management;
using FEAR.Domain.Model;
using FEAR.Domain.Model.InvestigationStore;
using FEAR.Host.Domain.Api.Investigation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using System.Text.Json;
using MudBlazor;
using FEAR.Blazor.Shared.Components.Wizards;
using FEAR.Admin.Components.Case;

namespace FEAR.Admin.Pages.Investigation
{
    [Authorize]
    public partial class Home
    {
        [Inject] InvestigationService InvestigationService { get; set; }

        [Inject] IDialogService DialogService { get; set; }

        QueryInvestigations.Response InvestigationResponse { get; set; } = new QueryInvestigations.Response();

        InvestigationEditor refInvestigationEditor { get; set; }

        protected override Func<InvestigationInfo, bool> QuickFilters => x =>
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
            var caseWizardModel = new CaseWizardModel
            {
                InvestigationInfo = item,
                Configuration = await InvestigationService.GetInvestigationConfiguration(item)
            };

            await OpenCaseWizard(item, caseWizardModel.Configuration);
        }

        private async Task EditInvestigationQueries(InvestigationInfo item)
        {
            var parameters = new DialogParameters
            {
                { "Investigation", item }
            };
            var options = new DialogOptions
            {
                MaxWidth = MaxWidth.Large,
                FullWidth = true,
                CloseButton = true,
                CloseOnEscapeKey = true
            };
            await DialogService.ShowAsync<EditQueriesDialog>("Edit Investigation Queries", parameters, options);
        }

        private async Task EditInvestigationUsers(InvestigationInfo item)
        {
            var parameters = new DialogParameters
            {
                { "Investigation", item}
            };
            var options = new DialogOptions
            {
                MaxWidth = MaxWidth.Medium,
                FullWidth = true,
                CloseButton = true,
                CloseOnEscapeKey = true
            };
            await DialogService.ShowAsync<EditUsersDialog>("Edit Investigation Users", parameters, options);
        }

        private async Task DeleteInvestigation(InvestigationInfo item)
        {
            await InvestigationService.DeleteInvestigation(item);
            LoadInvestigations();
        }

        private async Task RestartInvestigationGraphService(InvestigationInfo item)
        {
            await InvestigationService.RestartInvestigationGraphService(item.Name);
        }

        private async Task StopInvestigationGraphService(InvestigationInfo item)
        {
            await InvestigationService.StopInvestigationGraphService(item.Name);
        }

        private async Task StartInvestigationGraphService(InvestigationInfo item)
        {
            await InvestigationService.StartInvestigationGraphService(item.Name);
        }

        private async Task<string> GetInvestigationGraphServiceState(InvestigationInfo item)
        {
            return await InvestigationService.GetInvestigationGraphServiceState(item.Name);
        }

        private async Task CreateCaseWizard()
        {
            await OpenCaseWizard(new InvestigationInfo() { Name = "New Investigation", Description = String.Empty, CaseNumber = String.Empty, CaseStatus = "Active" },
                new CaseConfiguration());
        }
        private async Task OpenCaseWizard(InvestigationInfo item, CaseConfiguration config)
        {
            var parameters = new DialogParameters();
            parameters.Add(nameof(WizardBase<object>.Model), new CaseWizardModel()
            {
                InvestigationInfo = item,
                Configuration = config
            });

            var dialogOptions = new DialogOptions()
            {
                MaxWidth = MaxWidth.Large,
                FullWidth = true,
                CloseButton = true,
                CloseOnEscapeKey = true
            };

            var dialog = DialogService.Show<CaseWizardController>(item.InvestigationId != Guid.Empty ? $"Edit {item.Name}" : "New Investigation", parameters, dialogOptions);
            var result = await dialog.Result;

            if (!result.Canceled)
            {
                var model = (CaseWizardModel)result.Data;
                await SaveInvestigation(model.InvestigationInfo, model.Configuration);
                LoadInvestigations();
            }
        }

        private async Task SaveInvestigation(InvestigationInfo updatedValue, CaseConfiguration config)
        {
            // Need to save the configuration as well somehow
            await InvestigationService.SaveInvestigation(updatedValue, config);
        }

        private void ViewOntology(InvestigationInfo item)
        {
            var parameters = new DialogParameters
            {
                { nameof(ViewOntologyDialog.Investigation), item },
                { nameof(ViewOntologyDialog.InvestigationName), item.Name }
            };

            var options = new DialogOptions
            {
                MaxWidth = MaxWidth.Large,
                FullWidth = true,
                CloseButton = true,
                CloseOnEscapeKey = true
            };

            DialogService.Show<ViewOntologyDialog>($"Ontology - {item.Name}", parameters, options);
        }

        private void ViewInvestigationInfo(InvestigationInfo item)
        {
            var parameters = new DialogParameters
            {
                { nameof(InvestigationInfoDialog.Investigation), item },
                { nameof(InvestigationInfoDialog.InvestigationName), item.Name }
            };

            var options = new DialogOptions
            {
                MaxWidth = MaxWidth.Large,
                FullWidth = true,
                CloseButton = true,
                CloseOnEscapeKey = true
            };

            DialogService.Show<InvestigationInfoDialog>($"Info - {item.Name}", parameters, options);
        }

        private void ViewTelemetryLogs(InvestigationInfo item)
        {
            var parameters = new DialogParameters
            {
                { "InvestigationId", item.InvestigationId },
                { "InvestigationName", item.Name }
            };

            var options = new DialogOptions
            {
                MaxWidth = MaxWidth.Large,
                FullWidth = true,
                CloseButton = true,
                CloseOnEscapeKey = true
            };

            DialogService.Show<TelemetryLogsDialog>("Investigation Telemetry", parameters, options);
        }

        private async Task ExportInvestigation(InvestigationInfo item)
        {
            var config = await InvestigationService.GetInvestigationConfiguration(item);
            var agentAdapterFields = await InvestigationService.GetConfigurationFields();
            var agentAdapterTypes = agentAdapterFields
                .FirstOrDefault(f => f.Name == "AgentAdapterTypes")?.FieldOptions
                ?? new List<ConfigurationFieldOption>();

            var parameters = new DialogParameters
            {
                { nameof(ExportCaseDialog.Investigation), item },
                { nameof(ExportCaseDialog.Configuration), config },
                { nameof(ExportCaseDialog.AgentAdapterTypes), agentAdapterTypes }
            };

            var options = new DialogOptions
            {
                MaxWidth = MaxWidth.Medium,
                FullWidth = true,
                CloseButton = true,
                CloseOnEscapeKey = true
            };

            await DialogService.ShowAsync<ExportCaseDialog>($"Export — {item.Name}", parameters, options);
        }

        private async Task ImportInvestigation()
        {
            var options = new DialogOptions
            {
                MaxWidth = MaxWidth.Small,
                FullWidth = true,
                CloseButton = true,
                CloseOnEscapeKey = true
            };

            var dialog = await DialogService.ShowAsync<ImportCaseDialog>("Import Case", new DialogParameters(), options);
            await dialog.Result;

            LoadInvestigations();
        }
    }
}