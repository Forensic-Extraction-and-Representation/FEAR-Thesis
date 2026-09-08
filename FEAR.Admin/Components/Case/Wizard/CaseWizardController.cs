using FEAR.Admin.Components.Case.Wizard.Steps;
using FEAR.Admin.Services.Management;
using FEAR.Blazor.Shared.Components.Wizards;
using FEAR.Host.Domain.Api.Investigation;
using Microsoft.AspNetCore.Components;

public class CaseWizardController : WizardBase<CaseWizardModel>
{
    [Inject] InvestigationService InvestigationService { get; set; }
    public List<ConfigurationField> ConfigurationFields { get; set; } = new List<ConfigurationField>();

    public override bool IsEditing => Model != null && Model.InvestigationInfo.InvestigationId != Guid.Empty;

    override protected async Task OnInitializedAsync()
    {
        Model.Configuration = await InvestigationService.GetInvestigationConfiguration(Model.InvestigationInfo);
        ConfigurationFields = await InvestigationService.GetConfigurationFields();
        Steps = new List<Type> {
            typeof(Welcome), typeof(CaseDetails), typeof(Directories), typeof(GraphSettings),
            typeof(ScriptRepositories), typeof(LibraryRepositories), typeof(PackageRepositories),
            typeof(ChatInference), typeof(Summary)
        };
        await base.OnInitializedAsync();
    }

    protected override async Task OnParametersSetAsync()
    {
        await base.OnParametersSetAsync();
    }

    protected override async Task OnCommitAsync()
    {
        Model.Configuration.InvestigationName = Model.InvestigationInfo.Name;
        await base.OnCommitAsync();
    }

    public ConfigurationField GetFieldOptions(string name)
    {
        var field = ConfigurationFields.FirstOrDefault(f => f.Name == name);
        if (field != null)
        {
            return field;
        }

        return default;
    }
}
