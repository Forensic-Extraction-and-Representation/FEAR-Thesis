using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using MudBlazor;
using MudBlazor.Utilities;

namespace FEAR.Blazor.Shared.Components.Forms
{
    public partial class EditableList<T> : MudDebouncedInput<T>
    {
        protected string Classname =>
           new CssBuilder("mud-input-input-control")
               .AddClass(Class)
               .Build();

        /// <summary>
        /// The reference to the underlying <see cref="MudInput{T}"/> component.
        /// </summary>
        public MudTextField<string>? InputReference { get; private set; }

        /// <summary>
        /// The type of input collected by this component.
        /// </summary>
        /// <remarks>
        /// Defaults to <see cref="InputType.Text"/>.  Represents a valid HTML5 input type.
        /// </remarks>
        [Parameter]
        [Category(CategoryTypes.FormComponent.Behavior)]
        public InputType InputType { get; set; } = InputType.Text;

        /// <summary>
        /// Shows a button to clear this input's value.
        /// </summary>
        /// <remarks>
        /// Defaults to <c>false</c>.
        /// </remarks>
        [Parameter]
        [Category(CategoryTypes.FormComponent.Behavior)]
        public bool Clearable { get; set; } = false;

        /// <summary>
        /// The icon to display when <see cref="Clearable"/> is <c>true</c>.
        /// </summary>
        /// <remarks>
        /// Defaults to <see cref="Icons.Material.Filled.Clear"/>.
        /// </remarks>
        [Parameter]
        [Category(CategoryTypes.FormComponent.Appearance)]
        public string ClearIcon { get; set; } = Icons.Material.Filled.Clear;

        /// <summary>
        /// Occurs when the clear button is clicked.
        /// </summary>
        /// <remarks>
        /// When clicked, the <see cref="MudBaseInput{T}.Text"/> and <see cref="MudBaseInput{T}.Value"/> properties are reset.
        /// </remarks>
        [Parameter]
        public EventCallback<MouseEventArgs> OnClearButtonClick { get; set; }

        protected async Task HandleContainerClick()
        {
            if (!_isFocused && IsJSRuntimeAvailable && InputReference != null)
            {
                await InputReference.FocusAsync();
            }
        }

        protected override async Task InvokeKeyUpAsync(KeyboardEventArgs obj)
        {
            if (obj.Key == "Enter" && InputReference != null)
            {
                // Add the current text to the list and clear the input
                if (!string.IsNullOrWhiteSpace(InputReference.Text))
                {
                    await AddItem(InputReference.Text);
                    return;
                }
            }

            await base.InvokeKeyUpAsync(obj);
        }
    }
}