using EventAggregator.Blazor;
using FEAR.Domain.Model.InvestigationStore;
using FEAR.WASM.Model;
using FEAR.WASM.Providers;
using FEAR.WASM.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace FEAR.WASM.Pages
{
    [Authorize]
    public partial class WebUI
    {
        [Inject] protected IJSRuntime JS { get; set; } = default!;
        [Inject] protected IEventAggregator Mediator { get; set; } = default!;

        [Parameter]
        public string? InvestigationName { get; set; }

        public InvestigationInfo InvestigationDetails { get; private set; }

        [Inject] WebUIConfiguration Configurations { get; set; }
        [Inject] InvestigationService InvestigationService { get; set; }
        [Inject] IColorMapProvider ColorMapProvider { get; set; }
        [Inject] QueryService QueryService { get; set; }

        protected override async Task OnParametersSetAsync()
        {
            Configurations.InvestigationName = InvestigationName;
            InvestigationDetails = await InvestigationService.GetInvestigationByName(InvestigationName);
            await ColorMapProvider.InitializeAsync(InvestigationDetails.InvestigationId);
        }
    }
}