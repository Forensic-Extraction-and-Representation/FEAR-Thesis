using Microsoft.AspNetCore.Components;

namespace FEAR.Blazor.Shared.Components.Wizards
{
    public abstract class WizardContext : ComponentBase
    {
        internal abstract void AddStep(WizardStep step);
        public abstract void StateChange();
        public abstract object? ModelObject { get; }
    }


}
