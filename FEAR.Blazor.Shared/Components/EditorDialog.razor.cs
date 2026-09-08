using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace FEAR.Blazor.Shared.Components
{
    public partial class EditorDialog<T>
    {
        [Parameter] public EventCallback<T> OnSave { get; set; }
        [Parameter] public EventCallback<bool> OnClose { get; set; }
        [Parameter] public Func<bool> IsSaveDisabled { get; set; }
        [Parameter] public Func<T, bool> CloseOnSave { get; set; } = (t) => true;
        [Parameter] public RenderFragment<T> FormContentTemplate { get; set; }
        [Parameter] public RenderFragment<T> HeaderTemplate { get; set; }

        [Inject] IDialogService DialogService { get; set; }

        /// <summary>
        /// If true, a warning dialog will be shown if the user attempts to close the dialog with unsaved changes.
        /// </summary>
        [Parameter] public bool WarningOnClose { get; set; } = true;

        [Parameter]
        public DialogOptions DialogOptions { get; set; } = new() { FullWidth = true };

        public T EditingEntry { get; set; }

        private bool _visible;
        private bool _loading = false;
        private bool success;
        private MudForm form;
        

        public void OpenDialog() => _visible = true;

        public async Task CloseDialog(bool saved)
        {
            if (WarningOnClose && !saved)
            {
                bool? result = await DialogService.ShowMessageBox(
                    "Warning",
                    "Unsaved changes will be lost. Do you wish to continue?",
                    yesText: "Yes", cancelText: "Cancel");

                if (result == null)
                    return;
            }

            if (OnClose.HasDelegate)
                await OnClose.InvokeAsync(saved);

            _visible = false;
        }

        protected override async Task OnInitializedAsync()
        {
            StateHasChanged();
        }

        private async Task SaveEntry()
        {
            _loading = true;
            bool canClose = CloseOnSave(EditingEntry);
            await OnSave.InvokeAsync(EditingEntry);
            _loading = false;

            if (canClose)
                await CloseDialog(true);
        }


        public void BeginEdit(T item)
        {
            EditingEntry = item;
            OpenDialog();

        }
    }
}