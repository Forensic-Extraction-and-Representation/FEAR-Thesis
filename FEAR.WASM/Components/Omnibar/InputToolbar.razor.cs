using FEAR.WASM.Interactions;
using FEAR.WASM.Services;
using Microsoft.AspNetCore.Components;

namespace FEAR.WASM.Components.Omnibar
{
    public partial class InputToolbar
    {
        private string _selectedInputType  = "query";
        private Dictionary<string, bool> _checkOptions = new Dictionary<string, bool>();

        protected override async Task OnParametersSetAsync()
        {
        }

        protected override void OnInitialized()
        {
            _checkOptions.Add("IncludeGraph", false);
            _checkOptions.Add("QueryHelp", false);
            _checkOptions.Add("AddAsLayer", false);
        }

        public async Task ChangeInputType(string e)
        {
            _selectedInputType = e;
            Mediator.PublishAsync(new ToolbarEvent<string>("RequestType", e));
            StateHasChanged();
        }

        public async Task ToggleOption(string option)
        {
            bool current = _checkOptions[option];

            _checkOptions[option] = !current;
            Mediator.PublishAsync(new ToolbarEvent<bool>(option, !current));
            StateHasChanged();
        }
    }
}