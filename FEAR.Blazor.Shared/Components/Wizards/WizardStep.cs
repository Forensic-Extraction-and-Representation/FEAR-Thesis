using FEAR.Blazor.Shared.Components.Wizards;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using MudBlazor;
using System;
using System.Threading.Tasks;

namespace FEAR.Blazor.Shared.Components.Wizards
{
    public abstract class WizardStep : ComponentBase
    {
        [CascadingParameter]
        protected internal WizardContext Wizard { get; set; } = default!;

        public virtual bool RequiredValidation => true;

        protected MudForm InternalMudForm;
        protected bool MudFormValidated = false;

        public object? ModelObject => Wizard?.ModelObject;

        protected virtual void UpdateValidationState(bool isValid)
        {
            if (RequiredValidation)
            {
                MudFormValidated = isValid;
                NotifyStateChanged();
            }
        }

        /// <summary>
        /// Determines if the step is currently valid.
        /// default is true. Override to implement custom validation logic.
        /// </summary>
        public virtual Task<bool> IsValidAsync()
        {
            InternalMudForm.Validate();
            MudFormValidated = InternalMudForm.IsValid;
            StateHasChanged();
            return Task.FromResult(MudFormValidated);
        }

        protected override void OnInitialized()
        {
            base.OnInitialized();
            if (Wizard != null)
            {
                Wizard.AddStep(this);
            }
        }

        /// <summary>
        /// Helper method to notify the wizard that the validity of this step has changed.
        /// </summary>
        protected void NotifyStateChanged()
        {
            Wizard?.StateChange();
        }

        public virtual void OnWizardStepChanging()
        {
        }
    }

    public abstract class WizardStep<TWizardType, TModel> : WizardStep
        where TWizardType : WizardBase<TModel>
    {
        public new TModel? Model => (TModel?)base.ModelObject;

        public new TWizardType? Wizard => (TWizardType?)base.Wizard;
        protected override void OnInitialized()
        {
            base.OnInitialized();
        }
    }


}
